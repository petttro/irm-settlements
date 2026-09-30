using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Domain.DomainEvents;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Application.DomainEventHandlers;

public static class AppealPriceChangedEventHandler
{
    public static async Task Handle(
        AppealPriceChangedEvent reportEvent,
        IMessagePublisher messagePublisher,
        ILogger<AppealPriceChangedEvent> logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Appeal price changed. CouponNumber: {CouponNumber}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                reportEvent.CouponNumber, reportEvent.OldPrice, reportEvent.NewPrice);

        var message = KafkaMessageContainer<AppealPriceChangedEvent>.Build(reportEvent);

        // Отправляем в кафку
        await messagePublisher.PublishAsync( reportEvent.CouponNumber, message);
    }
}
