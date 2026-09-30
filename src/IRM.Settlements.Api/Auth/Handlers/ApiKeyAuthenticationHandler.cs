using System.Security.Claims;
using System.Text.Encodings.Web;
using IRM.Settlements.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Api.Auth.Handlers;

/// <summary>
/// Обработчик аутентификации с использованием API ключа.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    /// <summary>
    /// Название схемы аутентификации.
    /// </summary>
    public const string SchemeName = "ApiKey";

    /// <inheritdoc />
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue("x-api-key", out var apiKeyHeader))
            return Task.FromResult(AuthenticateResult.Fail("Missing 'x-api-key' header."));

        if (!string.Equals(apiKeyHeader, Options.ApiKey, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Integration"),
            new Claim(ClaimTypes.Role, nameof(IrmRoles.Integration))
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
