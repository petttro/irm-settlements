using IRM.Settlements.Application.Abstractions.Excel;
using IRM.Settlements.Infrastructure.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Settlements.Infrastructure.Excel;

public static class DependencyInjection
{
    public static IServiceCollection AddExcelExporter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ExcelExportSettings>()
            .Bind(configuration.GetSection(ExcelExportSettings.SectionName))
            .Required(x => x.ProtectionPassword)
            .ValidateOnStart();

        services.AddScoped<IExcelExporter, ExcelExporter>();

        return services;
    }
}
