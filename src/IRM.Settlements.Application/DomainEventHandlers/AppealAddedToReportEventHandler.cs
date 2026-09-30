using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class AppealAddedToReportEventHandler
{
    public static async Task Handle(
        AppealAddedToReportEvent reportEvent,
        IMessagePublisher messagePublisher,
        ILogger<AppealAddedToReportEvent> logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Appeal added to report. ReportId: {ReportId}, CouponNumber: {CouponNumber}",
                reportEvent.ReportId, reportEvent.CouponNumber);

        var message = KafkaMessageContainer<AppealAddedToReportEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync(reportEvent.CouponNumber, message);
    }
}
