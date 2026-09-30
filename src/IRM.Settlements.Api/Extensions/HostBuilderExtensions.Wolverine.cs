using IRM.Settlements.Api.PolicyHandlers;
using IRM.Settlements.Api.Settings;
using IRM.Settlements.Application;
using IRM.Settlements.Application.Integrations.IntegrationEvents;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Infrastructure.Kafka.Settings;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Postgresql;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей в Host.
/// </summary>
public static partial class HostBuilderExtensions
{
    /// <summary>
    /// Регистрация Wolverine.
    /// </summary>
    /// <param name="host">Хост приложения.</param>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    /// <returns>Хост приложения.</returns>
    /// <exception cref="MissingSettingException">Не заданы настройки политики повторных попыток.</exception>
    public static ConfigureHostBuilder AddWolverine(this ConfigureHostBuilder host,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        const string connectionStringName = "SettlementsConnectionString";
        var settlementsDbConnectionString = configuration.GetConnectionString(connectionStringName);

        if (!hostEnvironment.IsPipeEnvironment())
            MissingConnectionStringException.ThrowIfNull(settlementsDbConnectionString!, connectionStringName);

        var retryPolicySettings = configuration.GetSection(nameof(WolverineRetryPolicySettings)).Get<WolverineRetryPolicySettings>();
        if (retryPolicySettings == null)
            throw new MissingSettingException(typeof(WolverineRetryPolicySettings), nameof(WolverineRetryPolicySettings));

        if (retryPolicySettings.InitialDelaySeconds <= 0)
            throw new MissingSettingException(
                typeof(WolverineRetryPolicySettings),
                nameof(WolverineRetryPolicySettings.InitialDelaySeconds));

        if (retryPolicySettings.IncrementSeconds <= 0)
            throw new MissingSettingException(typeof(WolverineRetryPolicySettings), nameof(WolverineRetryPolicySettings.IncrementSeconds));

        if (retryPolicySettings.MaxDelaySeconds <= 0)
            throw new MissingSettingException(
                typeof(WolverineRetryPolicySettings),
                nameof(WolverineRetryPolicySettings.MaxDelaySeconds));

        var producerSettings = configuration.GetSection(KafkaProducerSettings.SectionName).Get<KafkaProducerSettings>();
        if (producerSettings == null)
            throw new MissingSettingException(typeof(KafkaProducerSettings), nameof(KafkaProducerSettings));

        host.UseWolverine(opts =>
        {
            if (hostEnvironment.IsIntegrationTests())
            {
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
                opts.UseRuntimeCompilation();
            }

            if (!hostEnvironment.IsIntegrationTests())
            {
                // Статическая генерация обработчиков для снижения потребления RAM в Pod.
                // dotnet run -- codegen write && dotnet build && dotnet publish
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;
                opts.ApplicationAssembly = typeof(Program).Assembly;

                // Каждый обработчик события сам по себе. В случае если один из нескольких обработчиков упадет,
                // retry-и будут повторятся только для него.
                opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

                // Одно и тоже событие для разных обработчиков хранится отдельно в outbox.
                opts.Durability.MessageIdentity = MessageIdentity.IdAndDestination;

                // Используем SQL Server для персистентности сообщений.
                opts.PersistMessagesWithPostgresql(settlementsDbConnectionString!);

                // Успешные события хранятся 5 минут прежде чем быть удаленными.
                opts.Durability.KeepAfterMessageHandling = 5.Minutes();
                opts.LocalQueue("durable_queue")
                    .UseDurableInbox()
                    .MaximumParallelMessages(2);

                opts.PublishMessage<AppealUpdatedEvent>()
                    .ToLocalQueue("durable_queue");

                opts.Durability.InboxStaleTime = 5.Minutes();

                // Конфигурация продюсера кафки с outbox
                opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                opts.Durability.OutboxStaleTime = 1.Hours();

                opts.UseKafka(producerSettings.BootstrapServers)
                    .ConfigureProducers(producer =>
                    {
                        producer.BootstrapServers = producerSettings.BootstrapServers;
                        producer.SecurityProtocol = producerSettings.SecurityProtocol;
                        producer.SaslMechanism = producerSettings.SaslMechanism;
                        producer.SslCaLocation = producerSettings.SslCaLocation;
                        producer.SaslUsername = producerSettings.Username;
                        producer.SaslPassword = producerSettings.Password;
                    });

                // Чтобы сообщения отправляемые в кафку сериализовались в CamelCase
                opts.UseSystemTextJsonForSerialization(options =>
                    options.PropertyNamingPolicy = null);

                var settlementsTopicName = producerSettings.TopicName;

                // Маршрутизация сообщений в топики kafka
                opts.PublishMessage<KafkaMessageContainer<AppealAddedToReportEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<AppealRemovedFromReportEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<AppealPriceChangedEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<ReportCreatedEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<ReportDeletedEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<ReportSentToPaymentEvent>>()
                    .ToKafkaTopic(settlementsTopicName);

                opts.PublishMessage<KafkaMessageContainer<ReportPaidEvent>>()
                    .ToKafkaTopic(settlementsTopicName);
            }

            opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
            // Поиск обработчиков для сообщений.
            opts.Discovery.IncludeAssembly(typeof(IApplicationAssemblyMarker).Assembly);

            // Для событий устанавливается Retry policy.
            opts.Policies.Add(new EventRetryPolicy(
                TimeSpan.FromSeconds(retryPolicySettings.InitialDelaySeconds),
                TimeSpan.FromSeconds(retryPolicySettings.IncrementSeconds),
                TimeSpan.FromSeconds(retryPolicySettings.MaxDelaySeconds)
            ));
        });

        return host;
    }
}
