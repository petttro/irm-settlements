using IRM.Settlements.Api.Auth.Handlers;
using IRM.Settlements.Api.Settings;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Infrastructure.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    private const string AuthenticationScheme = "JwtOrApiKeyScheme";
    private const string AuthorizationPolicy = "JwtOrApiKey";
    private const string ApiKeyHeaderName = "x-api-key";

    /// <summary>
    /// Регистрация аутентификации и авторизации.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов.</returns>
    /// <exception cref="MissingSettingException">Настройки JWT не найдены.</exception>
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Required(x => x.IssuerSigningKeySecret)
            .Required(x => x.TokenDecryptionKeySecret)
            .ValidateOnStart();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
            })
            .AddPolicyScheme(AuthenticationScheme, "JWT or ApiKey", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey(ApiKeyHeaderName))
                        return ApiKeyAuthenticationHandler.SchemeName;

                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                                  ?? throw new MissingSettingException(typeof(JwtSettings), nameof(JwtSettings));

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = jwtSettings.IssuerSigningKey,
                    TokenDecryptionKey = jwtSettings.EncryptionEnabled ? jwtSettings.TokenDecryptionKey : default,
                    RoleClaimType = jwtSettings.RoleClaimType,
                    NameClaimType = jwtSettings.NameClaimType,
                    ValidateIssuerSigningKey = true
                };
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, options =>
                {
                    var apiKeySettings = configuration.GetSection(ApiKeySettings.SectionName).Get<ApiKeySettings>()
                                         ?? throw new MissingSettingException(typeof(ApiKeySettings), nameof(ApiKeySettings));

                    options.ApiKey = apiKeySettings.ApiKey;
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
