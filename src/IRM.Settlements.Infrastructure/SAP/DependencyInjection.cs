using System.ServiceModel;
using IRM.Settlements.Application.Abstractions.SAP;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Infrastructure.Common.Options;
using IRM.Settlements.Infrastructure.SAP.Behaviors;
using IRM.Settlements.Infrastructure.SAP.Clients;
using IRM.Settlements.Infrastructure.SAP.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using zmv_ws_get_prices_for_sp_bi;

namespace IRM.Settlements.Infrastructure.SAP;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрация клиента SAP.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов.</returns>
    /// <exception cref="MissingSettingException">Настройки не найдены в конфигурации.</exception>
    public static IServiceCollection AddSap(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISapClient, SapClient>();
        services.AddScoped<ISapIntegrationService, SapIntegrationService>();

        services.AddOptions<SapPriceSettings>()
            .Bind(configuration.GetSection(SapPriceSettings.SectionName))
            .Required(x => x.Url)
            .Required(x => x.UserName)
            .Required(x => x.Password)
            .ValidateOnStart();

        services.AddTransient<ZMV_WS_GET_PRICES_FOR_SP>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SapClient>>();
            var settings = sp.GetRequiredService<IOptions<SapPriceSettings>>().Value;

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var service = new ZMV_WS_GET_PRICES_FOR_SPClient(binding, new EndpointAddress(settings.Url));
            service.Endpoint.EndpointBehaviors.Add(new JsonSoapLoggingEndpointBehavior(logger));
            service.ClientCredentials.UserName.UserName = settings.UserName;
            service.ClientCredentials.UserName.Password = settings.Password;

            return service;
        });

        services.AddResiliencePipeline("sap-retry", pipeline =>
        {
            pipeline.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Linear,

                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutException>()
                    .Handle<CommunicationException>()
                    .Handle<TaskCanceledException>()
            });
        });

        return services;
    }
}
