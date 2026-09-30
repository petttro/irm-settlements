namespace IRM.Settlements.Application.Integrations.IntegrationEvents;

public record KafkaMessageContainer<T>
{
    public int Id { get; init; }

    public required List<DomainEventData<T>> Events { get; init; }

    public static KafkaMessageContainer<T> Build(T domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new KafkaMessageContainer<T>
        {
            Id = 0,
            Events =
            [
                new DomainEventData<T>
                {
                    DomainEvent = domainEvent,
                    DomainEventType = domainEvent.GetType().Name
                }
            ]
        };
    }
}

public record DomainEventData<T>
{
    public required T DomainEvent { get; init; }

    public required string DomainEventType { get; init; }
}
