using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.DomainEvents.Common;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class UpdateReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task PutReport_Success()
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        // Arrange
        var createReportRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.ParseExact("2026-01-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-03-01", "yyyy-MM-dd")
        };

        var report = await CreateDefaultReport(createReportRequest);

        var updateReportRequest = new UpdateReportRequest
        {
            ServiceDateFrom = DateOnly.ParseExact("2026-02-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-05-01", "yyyy-MM-dd")
        };

        // Act
        var createResponse = await HttpClient.PutAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();

        reportResult.ServiceDateFrom.ShouldBe(updateReportRequest.ServiceDateFrom);
        reportResult.ServiceDateTo.ShouldBe(updateReportRequest.ServiceDateTo);

        reportResult.ItemsCount.ShouldBe(2);

        reportResult.TotalCost.ShouldBeGreaterThan(1000);

        var actualItems = await GetReportItems(report.Id);
        actualItems.Data.Count.ShouldBe(2);

        actualItems.Data.Find(i => i.CouponNumber == testData!.Appeals[0].CouponNumber).ShouldBeNull();

        var item1 = actualItems.Data.Find(i => i.CouponNumber == testData!.Appeals[1].CouponNumber).ShouldNotBeNull();
        item1.AdditionalServices[0].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);
        item1.AdditionalServices[1].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);

        var item2 = actualItems.Data.Find(i => i.CouponNumber == testData!.Appeals[2].CouponNumber).ShouldNotBeNull();
        item2.AdditionalServices[0].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);
        item2.AdditionalServices[1].Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);

        var allAppeals = await GetAllAppealsAsync();

        var appeal1 = allAppeals.Find(a => a.CouponNumber == testData!.Appeals[0].CouponNumber);
        appeal1?.ReportId.ShouldBeNull();

        var appeal2 = allAppeals.Find(a => a.CouponNumber == testData!.Appeals[1].CouponNumber);
        appeal2?.ReportId.ShouldBe(report.Id);

        var appeal3 = allAppeals.Find(a => a.CouponNumber == testData!.Appeals[2].CouponNumber);
        appeal3?.ReportId.ShouldBe(report.Id);

        await Eventually(async () =>
        {
            var kafkaProducer = WebApplicationFactory.Services.GetRequiredService<IMessagePublisher>() as FakeMessagePublisher;
            kafkaProducer.ShouldNotBeNull();

            var deleteEvent = kafkaProducer.SentMessages
                .OfType<KafkaMessageContainer<AppealRemovedFromReportEvent>?>()
                .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealRemovedFromReport &&
                                     m.Events[0].DomainEvent.CouponNumber == appeal1?.CouponNumber);

            deleteEvent.ShouldNotBeNull();

            var addEvent = kafkaProducer.SentMessages
                .OfType<KafkaMessageContainer<AppealAddedToReportEvent>?>()
                .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealAddedToReport &&
                                     m.Events[0].DomainEvent.CouponNumber == appeal3?.CouponNumber &&
                                     m.Events[0].DomainEvent.ReportId == report.Id);

            addEvent.ShouldNotBeNull();
            addEvent.Events[0].DomainEventType.ShouldBe(nameof(AppealAddedToReportEvent));
            addEvent.Events[0].DomainEvent.EventId.ShouldNotBe(Guid.Empty);
            addEvent.Events[0].DomainEvent.OccurredAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        });
    }

    [Fact]
    public async Task PutReport_ServiceCenters_Success()
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        // Arrange
        var serviceCenterFilter = "32";

        var createReportRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.ParseExact("2026-01-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-12-01", "yyyy-MM-dd")
        };

        var report = await CreateDefaultReport(createReportRequest);

        var updateReportRequest = new UpdateReportRequest
        {
            ServiceDateFrom = DateOnly.ParseExact("2026-01-02", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-12-02", "yyyy-MM-dd"),
            ServiceCenters = [serviceCenterFilter]
        };

        // Act
        var createResponse = await HttpClient.PutAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();
        reportResult.ItemsCount.ShouldBe(1);

        var actualItems = await GetReportItems(report.Id);
        actualItems.Data.Count.ShouldBe(1);
        actualItems.Data.ShouldNotContain(i => i.ServiceCenterExternalId != serviceCenterFilter);
    }

    [Fact]
    public async Task PutReport_TheOnlyDate_Success()
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        // Arrange
        var createReportRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.ParseExact("2026-01-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-03-01", "yyyy-MM-dd")
        };

        var report = await CreateDefaultReport(createReportRequest);

        var updateReportRequest = new UpdateReportRequest
        {
            ServiceDateFrom = DateOnly.ParseExact("2026-01-02", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-01-02", "yyyy-MM-dd")
        };

        // Act
        var createResponse = await HttpClient.PutAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();
        reportResult.ItemsCount.ShouldBe(1);
    }

    [Fact]
    public async Task PutReport_EmptyReport_Returns400()
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        // Arrange
        var createReportRequest = new CreateReportRequest
        {
            ServiceCompanySapId = ServiceCompanyId,
            ServiceDateFrom = DateOnly.ParseExact("2026-01-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2026-03-01", "yyyy-MM-dd")
        };

        var report = await CreateDefaultReport(createReportRequest);

        var updateReportRequest = new UpdateReportRequest
        {
            ServiceDateFrom = DateOnly.ParseExact("2025-02-01", "yyyy-MM-dd"),
            ServiceDateTo = DateOnly.ParseExact("2025-05-01", "yyyy-MM-dd")
        };

        // Act
        var createResponse = await HttpClient.PutAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeFalse();
        var problemDetails = await createResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe("Отчёт не может быть пустым. Должен содержать хотя бы один элемент.");
    }
}
