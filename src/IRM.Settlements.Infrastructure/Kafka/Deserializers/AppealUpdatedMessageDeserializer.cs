using System.Text.Json;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Logging;
using Nuget.IRM.Kafka.Consumer.Deserializers;
using Nuget.IRM.Kafka.Consumer.Deserializers.Interfaces;
using Nuget.IRM.Kafka.Consumer.Enums;

namespace IRM.Settlements.Infrastructure.Kafka.Deserializers;

/// <summary>
/// Десериализатор для событий обновления ЗНУ из Kafka.
/// </summary>
public class AppealUpdatedMessageDeserializer : KafkaMessageCommonDeserializer<AppealUpdatedMessage>
{
    private readonly IJsonMessageProcessor _jsonMessageProcessor;

    private const string ObjectPath = "Snapshot";
    private const string EventType = "AppealUpdated";
    private const string SnapshotType = "AppealSnapshot";

    /// <summary>
    /// Создает новый объект <see cref="AppealUpdatedMessageDeserializer"/>
    /// </summary>
    /// <param name="jsonMessageProcessor">Сервис для валидации JSON.</param>
    /// <param name="logger">Логгер.</param>
    public AppealUpdatedMessageDeserializer(
        IJsonMessageProcessor jsonMessageProcessor,
        ILogger<AppealUpdatedMessageDeserializer> logger)
        : base(jsonMessageProcessor, logger)
    {
        _jsonMessageProcessor = jsonMessageProcessor;
        SetDeserializationTarget(ObjectPath, JsonElementType.JsonObject);
    }

    public override bool CanDeserialize(string jsonString)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonString);
            var root = jsonDocument.RootElement;

            // Проверка наличия нужного события
            if (!_jsonMessageProcessor.HasEventType(root, EventType))
                return false;

            // Проверка наличия объекта по заданному пути
            var targetElement = _jsonMessageProcessor.GetSnapshot(root, SnapshotType);
            return targetElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }

    protected override void ApplyCustomProcessing(JsonElement jsonElement, AppealUpdatedMessage message)
    {
        message.CreatedAt = message.CreatedAt.ToUniversalTime();

        // Преобразуем даты в UTC.
        // Из кафки приходят даты без указания таймзоны, но влокальном времени.
        message.Snapshot.Created = message.Snapshot.Created.MskToUtcMaybe();
        message.Snapshot.SaleDate = message.Snapshot.SaleDate.MskToUtcMaybe();
        message.Snapshot.AppealDate = message.Snapshot.AppealDate?.MskToUtcMaybe();
    }
}
