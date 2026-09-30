using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.DomainEvents.Common;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class UpdateReportItemsTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task PatchReport_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();
        var reportItems = await GetReportItems(report.Id);

        var deletedCouponNumber = reportItems.Data[2].CouponNumber;

        var updateReportRequest = new UpdateReportItemsRequest
        {
            Operations =
            [
                new ReportItemOperationRequest
                {
                    CouponNumber = deletedCouponNumber,
                    Type = Constants.OperationTypes.Delete
                }
            ]
        };

        // Act
        var createResponse = await HttpClient.PatchAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();

        var actualItems = await GetReportItems(report.Id);
        actualItems.Data.Count.ShouldBe(2);
        actualItems.Data.Find(i => i.CouponNumber == reportItems.Data[1].CouponNumber).ShouldNotBeNull();
        actualItems.Data.Find(i => i.CouponNumber == reportItems.Data[1].CouponNumber).ShouldNotBeNull();
        actualItems.Data.Find(i => i.CouponNumber == deletedCouponNumber).ShouldBeNull();

        reportResult.TotalCost.ShouldBeGreaterThan(1000);

        var allAppeals = await GetAllAppealsAsync();

        var deletedAppeal = allAppeals.Find(a => a.CouponNumber == deletedCouponNumber);
        deletedAppeal?.ReportId.ShouldBeNull();

        await Eventually(async () =>
        {
            var kafkaProducer = WebApplicationFactory.Services.GetRequiredService<IMessagePublisher>() as FakeMessagePublisher;
            kafkaProducer.ShouldNotBeNull();

            var deleteEvent = kafkaProducer.SentMessages
                .OfType<KafkaMessageContainer<AppealRemovedFromReportEvent>?>()
                .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealRemovedFromReport &&
                                     m.Events[0].DomainEvent.CouponNumber == deletedCouponNumber &&
                                     m.Events[0].DomainEvent.ReportId == report.Id);

            deleteEvent.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task PatchReport_DeleteAllItems_Returns400()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();
        var reportItems = await GetReportItems(report.Id);

        var updateReportRequest = new UpdateReportItemsRequest
        {
            Operations = reportItems.Data.Select(i =>
                new ReportItemOperationRequest
                {
                    CouponNumber = i.CouponNumber,
                    Type = Constants.OperationTypes.Delete
                }
            ).ToList()
        };

        // Act
        var patchResponse = await HttpClient.PatchAsJsonAsync($"reports/{report.Id}", updateReportRequest);
        await LogResponseOnFailureAsync(patchResponse);

        // Assert
        patchResponse.IsSuccessStatusCode.ShouldBeFalse();
        var problemDetails = await patchResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe("Отчёт не может быть пустым. Должен содержать хотя бы один элемент.");
    }
}
