namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для регистрации зависимостей в Host.
/// </summary>
public static partial class HostBuilderExtensions
{
    /// <summary>
    /// Регистрация Sentry.
    /// </summary>
    /// <param name="builder">Хост приложения.</param>
    public static void ConfigureSentry(this IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            var configuration = configBuilder.Build();

            if (bool.TryParse(configuration.GetSection("SentryEnabled").Value, out var sentryEnabled) && sentryEnabled)
            {
                builder.UseSentry();
            }
        });
    }
}
