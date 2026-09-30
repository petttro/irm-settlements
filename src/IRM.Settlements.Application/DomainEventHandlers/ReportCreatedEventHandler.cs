using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class ReportCreatedEventHandler
{
    public static async Task Handle(
        ReportCreatedEvent reportEvent,
        IMessagePublisher messagePublisher,
        ILogger<ReportCreatedEvent> logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Report created. ReportId: {ReportId}", reportEvent.ReportId);

        var message = KafkaMessageContainer<ReportCreatedEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync(reportEvent.ReportId.ToString(), message);
    }
}
