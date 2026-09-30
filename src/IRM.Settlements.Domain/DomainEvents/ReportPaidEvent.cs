using IRM.Settlements.Domain.DomainEvents.Common;

namespace IRM.Settlements.Domain.DomainEvents;

public record ReportPaidEvent(Guid ReportId, Guid PaymentId, decimal TotalCost) : DomainEvent
{
    public override string EventType => DomainEventTypes.ReportPaid;
}
