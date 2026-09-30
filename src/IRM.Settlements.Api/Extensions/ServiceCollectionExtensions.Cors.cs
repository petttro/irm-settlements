namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет поддержку CORS в сервисы.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="policyName">Название политики CORS.</param>
    /// <returns>Коллекция сервисов.</returns>
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration, string policyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        // Допустимые URI берутся из настроек для работы с UI IRM3 из контейнера.
        services.AddCors(options =>
        {
            var corsOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            if (corsOrigins is null || corsOrigins.Length == 0)
            {
                return;
            }

            options.AddPolicy(policyName,
                policyBuilder => policyBuilder
                    .WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("Content-Disposition"));
        });

        return services;
    }
}
