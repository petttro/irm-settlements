using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Infrastructure.Common.Options;
using IRM.Settlements.Infrastructure.Kafka.Deserializers;
using IRM.Settlements.Infrastructure.Kafka.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuget.IRM.Kafka.Consumer;
using Nuget.IRM.Kafka.Settings;
using Polly;
using Polly.Retry;

namespace IRM.Settlements.Infrastructure.Kafka;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрация Kafka consumers и producers.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов.</returns>
    /// <exception cref="MissingSettingException">Настройки не найдены в конфигурации.</exception>
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IRM.Settlements.Infrastructure.Kafka.Settings.KafkaProducerSettings>()
            .Bind(configuration.GetSection(Settings.KafkaProducerSettings.SectionName))
            .Required(x => x.BootstrapServers)
            .Required(x => x.TopicName)
            .Required(x => x.Username)
            .Required(x => x.Password)
            .Required(x => x.SslCaLocation)
            .Required(x => x.SecurityProtocol)
            .Required(x => x.SaslMechanism)
            .ValidateOnStart();

        services.AddScoped<IMessagePublisher, KafkaMessagePublisher>();

        // Consumers.
        var consumers = configuration.GetSection(nameof(KafkaConsumerSettings)).Get<List<KafkaConsumerSettings>>()
                        ?? throw new MissingSettingException(typeof(KafkaConsumerSettings), nameof(KafkaConsumerSettings));

        if (consumers.Count == 0)
            return services;

        services
            .UseWolverineDispatcher()
            .AddKafkaConsumerWorker<KafkaConsumerWorker>(
                consumers,
                handlingBuilder =>
                {
                    handlingBuilder
                        .Register<AppealUpdatedMessage, AppealUpdatedMessageDeserializer, AppealUpdatedEvent>(
                            message => new AppealUpdatedEvent(message));

                },
                c => c.ResiliencePipelineBuilder = builder => builder.AddRetry(new RetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 8,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(30),
                    Name = "KafkaConsumerRetry"
                }));

        return services;
    }
}
