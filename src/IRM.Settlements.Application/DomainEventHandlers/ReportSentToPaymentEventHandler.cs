using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class ReportSentToPaymentEventHandler
{
    public static async Task Handle(
        ReportSentToPaymentEvent reportEvent,
        ILogger<ReportSentToPaymentEvent> logger,
        IMessagePublisher messagePublisher)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Report sent to payment. ReportId: {ReportId}, PaymentId: {PaymentId}, TotalCost: {TotalCost}",
                reportEvent.ReportId,  reportEvent.PaymentId, reportEvent.TotalCost);

        var message = KafkaMessageContainer<ReportSentToPaymentEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync(reportEvent.ReportId.ToString(), message);
    }
}
