namespace IRM.Settlements.Application.Integrations.IntegrationEvents;

public record KafkaMessage
{
    public virtual string EventVersion => "1.0";

    public required string EventType { get; init; }

    public required Guid EventId { get; init; }

    public required DateTime OccurredAt { get; init; }
}
