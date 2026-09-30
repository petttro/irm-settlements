using System.Net.Http.Headers;
using IRM.Settlements.Application.Abstractions.PaymentGateway;
using IRM.Settlements.Domain;
using IRM.Settlements.Infrastructure.Common.Http.DelegatingHandlers;
using IRM.Settlements.Infrastructure.Common.Http.mTLS;
using IRM.Settlements.Infrastructure.Common.Http.Policies;
using IRM.Settlements.Infrastructure.Common.Options;
using IRM.Settlements.Infrastructure.PaymentGateway.HttpClients;
using IRM.Settlements.Infrastructure.PaymentGateway.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.PaymentGateway;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMTls(configuration);

        services.AddOptions<Finance3PSettings>()
            .Bind(configuration.GetSection(Finance3PSettings.SectionName))
            .Required(x => x.PaymentOrderUrl)
            .Required(x => x.PaymentStatusUrl)
            .Required(x => x.PayerInn)
            .Required(x => x.PayerKpp)
            .ValidateOnStart();

        services.AddScoped<IPaymentGateway, PaymentGateway>();
        services.AddTransient<LogHttpRequestDelegatingHandler>();

        services.AddHttpClient<PaymentOrderClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<Finance3PSettings>>().Value;

                client.BaseAddress = new Uri(options.PaymentOrderUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes.ApplicationJson));
            })
            .AddHttpMessageHandler<LogHttpRequestDelegatingHandler>()
            .AddPolicyHandler(HttpClientPolicyFactory.GetRetryPolicy(TimeSpan.FromSeconds(5)))
            .AddMTlsHttpHandler();

        services.AddHttpClient<PaymentStatusClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<Finance3PSettings>>().Value;

                client.BaseAddress = new Uri(options.PaymentStatusUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes.ApplicationJson));
            })
            .AddHttpMessageHandler<LogHttpRequestDelegatingHandler>()
            .AddPolicyHandler(HttpClientPolicyFactory.GetRetryPolicy(TimeSpan.FromSeconds(5)))
            .AddMTlsHttpHandler();

        return services;
    }
}
