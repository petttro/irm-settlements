using System.Net.Http.Json;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.DomainEvents.Common;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class DeleteReportItemTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task PatchReport_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();
        var reportItems = await GetReportItems(report.Id);
        var deletedReportItem = reportItems.Data[2];

        // Act
        var createResponse = await HttpClient.DeleteAsync($"reports/{report.Id}/items/{deletedReportItem.Id}");
        await LogResponseOnFailureAsync(createResponse);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();

        var actualItems = await GetReportItems(report.Id);
        actualItems.Data.Count.ShouldBe(2);
        actualItems.Data.Find(i => i.CouponNumber == reportItems.Data[1].CouponNumber).ShouldNotBeNull();
        actualItems.Data.Find(i => i.CouponNumber == reportItems.Data[1].CouponNumber).ShouldNotBeNull();
        actualItems.Data.Find(i => i.CouponNumber == deletedReportItem.CouponNumber).ShouldBeNull();

        report.TotalCost.ShouldNotBe(reportResult.TotalCost);
        reportResult.TotalCost.ShouldBeGreaterThan(1000);

        var allAppeals = await GetAllAppealsAsync();

        var deletedAppeal = allAppeals.Find(a => a.CouponNumber == deletedReportItem.CouponNumber);
        deletedAppeal?.ReportId.ShouldBeNull();

        await Eventually(async () =>
        {
            var kafkaProducer = WebApplicationFactory.Services.GetRequiredService<IMessagePublisher>() as FakeMessagePublisher;
            kafkaProducer.ShouldNotBeNull();

            var deleteEvent = kafkaProducer.SentMessages
                .OfType<KafkaMessageContainer<AppealRemovedFromReportEvent>?>()
                .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealRemovedFromReport &&
                                     m.Events[0].DomainEvent.CouponNumber == deletedReportItem.CouponNumber &&
                                     m.Events[0].DomainEvent.ReportId == report.Id);

            deleteEvent.ShouldNotBeNull();
            deleteEvent.Events[0].DomainEventType.ShouldBe(nameof(AppealRemovedFromReportEvent));
        });
    }
}
