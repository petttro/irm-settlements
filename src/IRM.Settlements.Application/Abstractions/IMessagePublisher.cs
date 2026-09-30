using IRM.Settlements.Application.Integrations.IntegrationEvents;

namespace IRM.Settlements.Application.Abstractions;

public interface IMessagePublisher
{
    ValueTask PublishAsync(string key, KafkaMessage message);

    ValueTask PublishAsync<T>(string key, KafkaMessageContainer<T> message);
}
