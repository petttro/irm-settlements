using System.Diagnostics;
using IRM.Settlements.Api.Settings;
using IRM.Settlements.Domain.Exceptions;
using Microsoft.FeatureManagement;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации OpenTelemetry.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Название сервиса в tempo.
    /// </summary>
    private const string ServiceName = "irm-settlements";

    /// <summary>
    /// Игнор обращений на указанные endpoint в телеметрии.
    /// </summary>
    private static readonly string[] IgnoredPaths =
    [
        "/health/ready",
        "/health/live",
        "/health/startup",
        "/metrics"
    ];

    /// <summary>
    /// Регистрация OpenTelemetry трейсинга.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов.</returns>
    /// <exception cref="MissingSettingException">Настройки телеметрии не найдены.</exception>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
        var usePostgreSqlTelemetry = featureManager
            .IsEnabledAsync("UsePostgreSqlTelemetry")
            .GetAwaiter()
            .GetResult();

        var useWolverineTelemetry = featureManager
            .IsEnabledAsync("UseWolverineTelemetry")
            .GetAwaiter()
            .GetResult();

        var openTelemetrySettings = configuration.GetSection(nameof(OpenTelemetrySettings)).Get<OpenTelemetrySettings>()
                                    ?? throw new MissingSettingException(typeof(OpenTelemetrySettings), nameof(OpenTelemetrySettings));

        services.AddOpenTelemetry().ConfigureResource(resource => resource
                .AddService(
                    serviceName: openTelemetrySettings.ServiceName ?? ServiceName,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Записывать детали HTTP запросов
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                            activity.SetTag("http.request.headers.user-agent",
                                httpRequest.Headers.UserAgent.ToString());
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.status_code", httpResponse.StatusCode);

                            if (httpResponse.StatusCode >= 400)
                            {
                                // Форсировать sampling для ошибок
                                activity.SetTag("error", true);
                                activity.ActivityTraceFlags |= ActivityTraceFlags.Recorded;
                            }
                        };

                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path.Value ?? "";

                            return !Array.Exists(IgnoredPaths, p =>
                                path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Трейсинг исходящих HTTP запросов
                        options.RecordException = true;
                    })
                    .AddOtlpExporter(options =>
                    {
                        if (!string.IsNullOrEmpty(openTelemetrySettings.OtlpEndpoint))
                        {
                            options.Endpoint = new Uri(openTelemetrySettings.OtlpEndpoint);
                        }
                        // Иначе используется переменная окружения OTEL_EXPORTER_OTLP_ENDPOINT
                    });

                if (usePostgreSqlTelemetry)
                {
                    tracing.AddNpgsql();
                }

                if (useWolverineTelemetry)
                {
                    tracing.AddSource("Wolverine");
                }

                // tracing.AddSource(AppealTelemetryService.SourceName);
            });

        return services;
    }
}
