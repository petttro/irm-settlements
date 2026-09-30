using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.Infrastructure.Postgres.Repositories;
using IRM.Settlements.Infrastructure.Postgres.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IRM.Settlements.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static readonly string ConnectionStringConfigName = "SettlementsConnectionString";

    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringConfigName);
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<SettlementsDbContext>(options =>
        {
            options.UseNpgsql(dataSource);
        });

        services.AddScoped<IAppealRepository, AppealRepository>();
        services.AddScoped<IServiceCompanyRepository, ServiceCompanyRepository>();
        services.AddScoped<IServiceCenterRepository, ServiceCenterRepository>();
        services.AddScoped<IMvzRepository, MvzRepository>();

        services.AddScoped<IReportNumberGenerator, ReportNumberGenerator>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
