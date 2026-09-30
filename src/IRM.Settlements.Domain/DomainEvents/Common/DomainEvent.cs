namespace IRM.Settlements.Domain.DomainEvents.Common;

public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public abstract string EventType { get; }
}
