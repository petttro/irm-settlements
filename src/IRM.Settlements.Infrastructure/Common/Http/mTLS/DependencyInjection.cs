using IRM.Settlements.Infrastructure.Common.Http.mTLS.Settings;
using IRM.Settlements.Infrastructure.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Settlements.Infrastructure.Common.Http.mTLS;

public static class DependencyInjection
{
    public static IServiceCollection AddMTls(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MTlsAuthSettings>()
            .Bind(configuration.GetSection(MTlsAuthSettings.SectionName))
            .Required(x => x.ClientCertificate.Data)
            .Required(x => x.ClientCertificate.Password)
            .ValidateOnStart();

        services.AddSingleton<MTlsHttpHandlerFactory>();
        services.AddSingleton<IMTlsCertificateProvider, MTlsCertificateProvider>();

        return services;
    }

    public static IHttpClientBuilder AddMTlsHttpHandler(this IHttpClientBuilder builder)
    {
        return builder.ConfigurePrimaryHttpMessageHandler(sp =>
            sp.GetRequiredService<MTlsHttpHandlerFactory>().Create());
    }
}
