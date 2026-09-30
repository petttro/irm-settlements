using IRM.Settlements.Domain.Entities.Interfaces;

namespace IRM.Settlements.Domain.Entities;

/// <inheritdoc/>
public abstract class DomainEntity<TKey> : IDomainEntity where TKey : notnull
{
    private readonly List<object> _events = [];

    public TKey Id { get; init; } = default!;

    public IReadOnlyCollection<object> DomainEvents => _events;

    protected void AddDomainEvent(object @event)
    {
        _events.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _events.Clear();
    }
}
