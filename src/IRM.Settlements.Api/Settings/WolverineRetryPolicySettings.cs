namespace IRM.Settlements.Api.Settings;

/// <summary>
/// Настройки политики повторных попыток для Wolverine Mediator.
/// </summary>
public sealed class WolverineRetryPolicySettings
{
    /// <summary>
    /// Начальная задержка в секундах.
    /// </summary>
    public int InitialDelaySeconds { get; set; }

    /// <summary>
    /// Приращение задержки в секундах.
    /// </summary>
    public int IncrementSeconds { get; set; }

    /// <summary>
    /// Максимальная задержка в секундах.
    /// </summary>
    public int MaxDelaySeconds { get; set; }
}
