using IRM.Settlements.Api.Serilog.Enrichers;
using Serilog;
using Serilog.Enrichers.Span;

namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей в Host.
/// </summary>
public static partial class HostBuilderExtensions
{
    /// <summary>
    /// Регистрация Serilog.
    /// </summary>
    /// <param name="host">Хост приложения.</param>
    /// <returns>Хост приложения.</returns>
    public static ConfigureHostBuilder AddSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, configuration) =>
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()
                    .Enrich.With<CategoryNameEnricher>()
                    .Enrich.With<CustomLogPropertiesEnricher>(),
            writeToProviders: true);

        return host;
    }
}
