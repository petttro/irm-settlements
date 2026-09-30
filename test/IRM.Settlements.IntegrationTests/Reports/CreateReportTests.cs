using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.DomainEvents.Common;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class CreateReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task CreateReport_Success()
    {
        // Arrange
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        var createRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceCenters = ["31"],
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01")
        };

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);

        // Act
        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();

        // Assert
        reportResult.ShouldNotBeNull();

        var expectedServiceCompany = testData!.ServiceCompanies.Find(c => c.SapId == ServiceCompanyId);

        reportResult.Number.ShouldStartWith("IRM0000");
        reportResult.Number.ShouldEndWith("/2026");
        reportResult.CreatedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        reportResult.TotalCost.ShouldBeGreaterThan(1000);
        reportResult.Status.ShouldBe(ReportStatus.Draft);
        reportResult.ServiceCompanyName.ShouldBe(expectedServiceCompany?.Name);
        reportResult.Permissions.ShouldBeEquivalentTo(
            new List<string>
            {
                PermissionTypes.ReportRead,
                PermissionTypes.ReportWrite,
                PermissionTypes.ReportDelete,
                PermissionTypes.ReportRecalculate,
                PermissionTypes.ReportSendToPayment,
                PermissionTypes.ReportExport,
                PermissionTypes.ServiceCenterRead
            });


        var expectedItemsCount = testData.Appeals.Count(a => createRequest.ServiceCenters.Contains(a.ServiceCenter.ExternalId));

        var reportItemsResult = await GetReportItems(reportResult.Id);
        reportItemsResult.ShouldNotBeNull();
        reportItemsResult.Data.Count.ShouldBe(expectedItemsCount);

        var allAppeals = await GetAllAppealsAsync();

        foreach (var item in reportItemsResult.Data)
        {
            var expectedAppeal = testData.Appeals.Find(c => c.CouponNumber == item.CouponNumber);
            expectedAppeal.ShouldNotBeNull();

            item.CouponNumber.ShouldBe(expectedAppeal.CouponNumber);
            item.OrderNumber.ShouldBe(expectedAppeal.OrderNumber);
            item.SaleOrderNumber.ShouldBe(expectedAppeal.SaleOrderNumber);

            item.SaleDate.ShouldBe(expectedAppeal.SaleDate);
            item.ServiceDate.ShouldBe(expectedAppeal.ServiceDate);

            item.ServiceName.ShouldBe(expectedAppeal.ServiceName);
            item.CityKisId.ShouldBe(expectedAppeal.CityKisId);
            item.CityName.ShouldBe(expectedAppeal.CityName);

            item.ServiceCenterExternalId.ShouldBe(expectedAppeal.ServiceCenter.ExternalId);
            item.ServiceCenterName.ShouldBe(expectedAppeal.ServiceCenter.Name);

            if (expectedAppeal.BsiStatus is  BsiStatus.Complete or BsiStatus.MaterialDefectAfterInstallation)
                item.Cost.ShouldBe(expectedAppeal.ServicePrice + FakeSapClient.SapPriceAddition);

            if (expectedAppeal.BsiStatus is BsiStatus.MaterialDefectAfterInstallation or BsiStatus.MaterialDefect)
            {
                item.AdditionalServices.Count.ShouldBe(2);
                item.AdditionalServices[0].Name.ShouldBe("Комф. Уст. Холод./Мор. с Перенав. дверей");
                item.AdditionalServices[0].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);

                item.AdditionalServices[1].Name.ShouldBe("Выезд мастера 1 зона");
                item.AdditionalServices[1].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);
            }

            var addedAppeal = allAppeals.Find(a => a.CouponNumber == item.CouponNumber);
            addedAppeal?.ReportId.ShouldBe(reportResult.Id);
        }

        await Eventually(async () =>
        {
            var kafkaProducer = WebApplicationFactory.Services.GetRequiredService<IMessagePublisher>() as FakeMessagePublisher;

            var reportCreatedEvent = kafkaProducer!.SentMessages
                .OfType<KafkaMessageContainer<ReportCreatedEvent>?>()
                .FirstOrDefault(r => r!.Events[0].DomainEvent.ReportId == reportResult.Id);

            reportCreatedEvent.ShouldNotBeNull();
            reportCreatedEvent.Events[0].DomainEventType.ShouldBe(nameof(ReportCreatedEvent));

            var addedCouponNumbers = testData.Appeals
                .Where(a => createRequest.ServiceCenters.Contains(a.ServiceCenter.ExternalId))
                .Select(a => a.CouponNumber).ToHashSet();

            foreach (var addedCouponNumber in addedCouponNumbers)
            {
                var addEvent = kafkaProducer.SentMessages
                    .OfType<KafkaMessageContainer<AppealAddedToReportEvent>?>()
                    .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealAddedToReport &&
                                         m.Events[0].DomainEvent.CouponNumber == addedCouponNumber &&
                                         m.Events[0].DomainEvent.ReportId == reportResult.Id);

                addEvent.ShouldNotBeNull();
                addEvent.Events[0].DomainEventType.ShouldBe(nameof(AppealAddedToReportEvent));
                addEvent.Events[0].DomainEvent.EventId.ShouldNotBe(Guid.Empty);
                addEvent.Events[0].DomainEvent.OccurredAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            }
        });
    }

    [Fact]
    public async Task CreateReport_MultipleServiceCenters_Success()
    {
        // Arrange
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        var createRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceCenters = ["31", "32"],
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01")
        };

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);

        // Act
        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();

        // Assert
        reportResult.ShouldNotBeNull();
        reportResult.ServiceCenterExternalIds.ShouldBeEquivalentTo(createRequest.ServiceCenters.ToList());

        var reportItemsResult = await GetReportItems(reportResult.Id);
        reportItemsResult.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateReport_AddDuplicateCouponNumber_ShouldFail()
    {
        // Arrange
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        var createRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01")
        };

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);


        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Act
        var failedCreateResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(failedCreateResponse);

        // Assert
        failedCreateResponse.IsSuccessStatusCode.ShouldBeFalse();
        failedCreateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await failedCreateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Detail.ShouldBe("ЗНУ не найдены с заданными параметрами отчета");
    }

    [Fact]
    public async Task CreateReport_AppealsNotFound_Returns400BadRequest()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var createRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01")
        };

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);


        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);

        createResponse.IsSuccessStatusCode.ShouldBeFalse();
        createResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await createResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Detail.ShouldBe("ЗНУ не найдены с заданными параметрами отчета");
    }

    [Fact]
    public async Task CreateReport_Admin_ReturnsForbidden()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var createRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01")
        };

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.Administrator);

        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);

        createResponse.IsSuccessStatusCode.ShouldBeFalse();
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var problem = await createResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Detail.ShouldBe($"Action {PermissionTypes.ReportCreate} not allowed for ServiceCompany {ServiceCompanyId}");
    }

    [Fact]
    public async Task CreateReport_MissingRequiredFields_Returns400BadRequest()
    {
        var createRequest = new CreateReportRequest();

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);

        var createResponse = await HttpClient.PostAsJsonAsync("reports", createRequest);
        await LogResponseOnFailureAsync(createResponse);

        createResponse.IsSuccessStatusCode.ShouldBeFalse();
        createResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await createResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Ошибка валидации");
        problem.Errors.ShouldContainKey(nameof(CreateReportRequest.ServiceCompanySapId));
        problem.Errors.ShouldContainKey(nameof(CreateReportRequest.ServiceDateFrom));
        problem.Errors.ShouldContainKey(nameof(CreateReportRequest.ServiceDateTo));
    }
}
