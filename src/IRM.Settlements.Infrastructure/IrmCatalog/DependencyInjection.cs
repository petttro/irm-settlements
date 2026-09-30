using System.Net.Http.Headers;
using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Domain;
using IRM.Settlements.Infrastructure.Common.Http.DelegatingHandlers;
using IRM.Settlements.Infrastructure.Common.Http.Policies;
using IRM.Settlements.Infrastructure.Common.Options;
using IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.IrmCatalog;

public static class DependencyInjection
{
    public static IServiceCollection AddIrmCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IrmCatalogSettings>()
            .Bind(configuration.GetSection(IrmCatalogSettings.SectionName))
            .Required(x => x.BaseUrl)
            .Required(x => x.ApiKey)
            .ValidateOnStart();

        services.AddScoped<IIrmCatalogService, IrmCatalogService>();

        services.AddTransient<LogHttpRequestDelegatingHandler>();
        services.AddHttpClient<IrmCatalogHttpClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<IrmCatalogSettings>>().Value;

                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes.ApplicationJson));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
            })
            .AddHttpMessageHandler<LogHttpRequestDelegatingHandler>()
            .AddPolicyHandler(HttpClientPolicyFactory.GetRetryPolicy(TimeSpan.FromSeconds(5)));

        return services;
    }
}
