using Microsoft.AspNetCore.Authentication;

namespace IRM.Settlements.Api.Auth.Handlers;

/// <summary>
/// Настройки для <see cref="ApiKeyAuthenticationHandler"/>
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// API ключ.
    /// </summary>
    public string? ApiKey { get; set; }
}
