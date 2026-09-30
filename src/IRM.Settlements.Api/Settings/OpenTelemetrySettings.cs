namespace IRM.Settlements.Api.Settings;

/// <summary>
/// Настройки OpenTelemetry.
/// </summary>
public sealed class OpenTelemetrySettings
{
    /// <summary>
    /// Название сервиса в tempo.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Адрес сервера OpenTelemetry.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Протокол телеметрии.
    /// </summary>
    public string? Protocol { get; set; }
}
