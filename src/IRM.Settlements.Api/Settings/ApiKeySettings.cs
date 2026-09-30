namespace IRM.Settlements.Api.Settings;

/// <summary>
/// Ключ API для работы с другими сервисами без JWT.
/// </summary>
public sealed class ApiKeySettings
{
    public static string SectionName { get; set; }  = nameof(ApiKeySettings);

    /// <summary>
    /// Ключ API для работы с другими сервисами без JWT.
    /// </summary>
    public required string ApiKey { get; set; }
}
