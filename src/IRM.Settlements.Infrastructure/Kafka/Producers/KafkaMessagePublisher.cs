using System.Diagnostics;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using Wolverine;

namespace IRM.Settlements.Infrastructure.Kafka.Producers;

public class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IMessageBus _bus;

    public KafkaMessagePublisher(IMessageBus bus)
    {
        _bus = bus;
    }

    public ValueTask PublishAsync(string key, KafkaMessage message)
    {
        var options = new DeliveryOptions
        {
            PartitionKey = key
        };

        options.WithHeader("event-type", message.EventType);
        options.WithHeader("event-version", message.EventVersion);

        return _bus.PublishAsync(message, options);
    }

    public ValueTask PublishAsync<T>(string key, KafkaMessageContainer<T> message)
    {
        var options = new DeliveryOptions
        {
            PartitionKey = key
        };

        options.WithHeader("trace_id", Activity.Current?.TraceId.ToString() ?? "n/a");
        options.WithHeader("EventsCount", "1");
        options.WithHeader("Events", string.Join(", ", message.Events.Select(x => x.DomainEventType)));

        return _bus.PublishAsync(message, options);
    }
}
