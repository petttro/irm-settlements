using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Settlements.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IAppealService, AppealService>()
            .AddScoped<IPriceService, PriceService>()
            .AddScoped<IPermissionsService, PermissionsService>();

        return services;
    }
}
