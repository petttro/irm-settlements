using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация проверок здоровья контейнера.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="hostEnvironment">Окружение хоста.</param>
    /// <returns>Коллекция сервисов.</returns>
    /// <exception cref="MissingSettingException">Отсутствует строка подключения к Redis.</exception>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        // Создаём временный ServiceProvider для получения IFeatureManager
        using var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        const string live = "live";
        const string ready = "ready";

        var healthChecksBuilder = services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { live });

        var readyChecksAdded = false;

        // Проверка DbContext
        if (featureManager.IsEnabledAsync("HealthCheckDatabase").GetAwaiter().GetResult())
        {
            healthChecksBuilder.AddDbContextCheck<SettlementsDbContext>(
                name: nameof(SettlementsDbContext).ToLower(System.Globalization.CultureInfo.CurrentCulture),
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { ready });
            readyChecksAdded = true;
        }

        // Если все ready проверки выключены, добавляем фиктивную проверку "здоров"
        if (!readyChecksAdded)
        {
            healthChecksBuilder.AddCheck(
                name: "ready-fallback",
                () => HealthCheckResult.Healthy(),
                tags: [ready]);
        }

        return services;
    }

    /// <summary>
    /// Добавляет endpoint-ы для проверки здоровья.
    /// </summary>
    /// <param name="app">WebApplication.</param>
    /// <returns>WebApplication.</returns>
    public static WebApplication MapHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            ResponseWriter = WriteJson
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = WriteJson
        });

        app.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = _ => false, // ничего не проверяем
            ResponseWriter = WriteJson
        });

        return app;
    }

    private static Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var result = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration,
                data = e.Value.Data,
                tags = e.Value.Tags
            })
        };

        return context.Response.WriteAsJsonAsync(result);
    }
}
