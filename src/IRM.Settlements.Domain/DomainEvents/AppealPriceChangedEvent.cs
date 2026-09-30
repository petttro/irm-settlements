using IRM.Settlements.Domain.DomainEvents.Common;

namespace IRM.Settlements.Domain.DomainEvents;

public record AppealPriceChangedEvent(string CouponNumber, decimal OldPrice, decimal NewPrice) : DomainEvent
{
    public override string EventType => DomainEventTypes.AppealPriceChanged;
}
