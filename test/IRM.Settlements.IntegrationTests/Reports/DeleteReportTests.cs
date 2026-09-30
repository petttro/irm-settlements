using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.DomainEvents.Common;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class DeletetReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task DeleteReport_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();
        var reportItems = await GetReportItems(report.Id);

        // Act
        var response = await HttpClient.DeleteAsync($"reports/{report.Id}");
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();

        var getResponse = await HttpClient.GetAsync($"reports/{report.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var notFoundProblem = await getResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblem.ShouldNotBeNull();
        notFoundProblem.Status.ShouldBe((int)HttpStatusCode.NotFound);

        var appeals = await GetAllAppealsAsync();
        foreach (var appeal in appeals)
        {
            appeal.ReportId.ShouldBeNull();
        }

        await Eventually(async () =>
        {
            var deletedCouponNumbers = reportItems.Data.Select(i => i.CouponNumber).ToHashSet();
            var kafkaProducer = WebApplicationFactory.Services.GetRequiredService<IMessagePublisher>() as FakeMessagePublisher;
            kafkaProducer.ShouldNotBeNull();

            var reportDeletedEvent = kafkaProducer.SentMessages
                .OfType<KafkaMessageContainer<ReportDeletedEvent>?>()
                .FirstOrDefault(e => e!.Events[0].DomainEvent.ReportId == report.Id);

            reportDeletedEvent.ShouldNotBeNull();
            reportDeletedEvent.Events[0].DomainEventType.ShouldBe(nameof(ReportDeletedEvent));

            foreach (var deletedCouponNumber in deletedCouponNumbers)
            {
                var deleteEvent = kafkaProducer.SentMessages
                    .OfType<KafkaMessageContainer<AppealRemovedFromReportEvent>?>()
                    .FirstOrDefault(m => m!.Events[0].DomainEvent.EventType == DomainEventTypes.AppealRemovedFromReport &&
                                         m.Events[0].DomainEvent.CouponNumber == deletedCouponNumber &&
                                         m.Events[0].DomainEvent.ReportId == report.Id);

                deleteEvent.ShouldNotBeNull();
                deleteEvent.Events[0].DomainEventType.ShouldBe(nameof(AppealRemovedFromReportEvent));
            }
        });
    }
}
