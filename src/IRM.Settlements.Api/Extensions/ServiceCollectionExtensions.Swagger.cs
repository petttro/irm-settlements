using IRM.Settlements.Api.Auth.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация Swagger для документации API.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="hostEnvironment">Окружение хоста.</param>
    /// <returns>Коллекция сервисов.</returns>
    public static IServiceCollection AddSwagger(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsProduction())
        {
            return services;
        }

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo { Title = "IRM.Settlement", Version = "v1" });
            setup.DescribeAllParametersInCamelCase();

            setup.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
                    Name = HeaderNames.Authorization,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
            setup.AddSecurityDefinition(
                ApiKeyAuthenticationHandler.SchemeName,
                new OpenApiSecurityScheme
                {
                    Description = @"API key Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
                    Name = HeaderNames.Authorization,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = ApiKeyAuthenticationHandler.SchemeName
                });

            setup.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });
            setup.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationHandler.SchemeName, document)] = []
            });
        });

        return services;
    }
}
