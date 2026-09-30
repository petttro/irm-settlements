using IRM.Settlements.Domain.DomainEvents.Common;

namespace IRM.Settlements.Domain.DomainEvents;

public record ReportCreatedEvent(Guid ReportId) : DomainEvent
{
    public override string EventType => DomainEventTypes.ReportCreated;
}
