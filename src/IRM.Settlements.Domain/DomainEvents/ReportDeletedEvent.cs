using IRM.Settlements.Domain.DomainEvents.Common;

namespace IRM.Settlements.Domain.DomainEvents;

public record ReportDeletedEvent(Guid ReportId) : DomainEvent
{
    public override string EventType => DomainEventTypes.ReportDeleted;
}
