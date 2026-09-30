using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class AppealRemovedFromReportEventHandler
{
    public static async Task Handle(
        AppealRemovedFromReportEvent reportEvent,
        IMessagePublisher messagePublisher,
        ILogger<AppealRemovedFromReportEvent> logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Appeal removed from report. ReportId: {ReportId}, CouponNumber: {CouponNumber}",
                reportEvent.ReportId,  reportEvent.CouponNumber);

        var message = KafkaMessageContainer<AppealRemovedFromReportEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync(reportEvent.CouponNumber, message);
    }
}
