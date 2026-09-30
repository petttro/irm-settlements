using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Infrastructure.Excel;
using IRM.Settlements.Infrastructure.Identity;
using IRM.Settlements.Infrastructure.IrmCatalog;
using IRM.Settlements.Infrastructure.Kafka;
using IRM.Settlements.Infrastructure.PaymentGateway;
using IRM.Settlements.Infrastructure.Postgres;
using IRM.Settlements.Infrastructure.SAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Settlements.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IUserContext, HttpUserContext>()

            .AddPostgres(configuration)
            .AddKafka(configuration)
            .AddSap(configuration)
            .AddPaymentGateway(configuration)
            .AddIrmCatalog(configuration)
            .AddExcelExporter(configuration);

        return services;
    }
}
