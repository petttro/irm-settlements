using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuget.IRM.Kafka.Consumer.Consuming.Interfaces;
using Nuget.IRM.Kafka.Consumer.Dispatchers.Interfaces;
using Nuget.IRM.Kafka.Consumer.Factories.Interfaces;
using Nuget.IRM.Kafka.Settings;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class FakeKafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FakeKafkaTopicMessageConsumer> _logger;

    public FakeKafkaTopicMessageConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<FakeKafkaTopicMessageConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // В тестах НЕ запускаем бесконечный consumer
    public Task StartConsuming(
        KafkaClusterSettings clusterSettings,
        KafkaConsumerTopicSettings topicSettings,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fake Kafka consumer started (no-op)");
        return Task.CompletedTask;
    }

    // ручной триггер обработки сообщения
    public async Task ProcessAsync(string message, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDeserializerFactory>();
        var deserializer = factory.GetDeserializerForMessage(message);

        if (deserializer == null)
        {
            _logger.LogWarning("No deserializer found");
            return;
        }

        var deserializedMessage = deserializer.Deserialize(message);

        if (deserializedMessage == null)
            return;

        var commandFactory = scope.ServiceProvider.GetRequiredService<ICommandFactory>();
        var command = commandFactory.CreateCommand(deserializedMessage);

        var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.DispatchAsync(command, cancellationToken);
    }
}
