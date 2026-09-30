using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IntegrationEvents;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class FakeMessagePublisher : IMessagePublisher
{
    public List<object> SentMessages { get; } = [];

    public ValueTask PublishAsync(string key, KafkaMessage message)
    {
        SentMessages.Add(message);
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishAsync<T>(string key, KafkaMessageContainer<T> message)
    {
        SentMessages.Add(message);
        return ValueTask.CompletedTask;
    }
}
