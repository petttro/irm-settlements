using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class ReportDeletedEventHandler
{
    public static async Task Handle(
        ReportDeletedEvent reportEvent,
        IMessagePublisher messagePublisher,
        ILogger<ReportDeletedEvent> logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Report deleted. ReportId: {ReportId}", reportEvent.ReportId);

        var message = KafkaMessageContainer<ReportDeletedEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync(reportEvent.ReportId.ToString(), message);
    }
}
