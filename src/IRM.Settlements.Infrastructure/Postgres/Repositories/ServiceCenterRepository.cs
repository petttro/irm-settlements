using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

public sealed class ServiceCenterRepository : Repository<ServiceCenter, int>, IServiceCenterRepository
{
    private readonly ILogger<ServiceCenterRepository> _logger;

    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных Settlement.</param>
    /// <param name="logger">Логгер</param>
    public ServiceCenterRepository(SettlementsDbContext context, ILogger<ServiceCenterRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public async Task<List<ServiceCenter>> SearchAsync(
        string? search, string serviceCompanySapId, int limit, CancellationToken cancellationToken = default)
    {
        var query = Query()
            .AsNoTracking()
            .Where(sc => sc.ServiceCompanySapId == serviceCompanySapId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(sc => EF.Functions.ILike(sc.Name, $"%{search}%"));

        return await query
            .OrderBy(sc => sc.Name)
            .Take(limit).ToListAsync(cancellationToken);
    }

    public override async Task UpsertAsync(ServiceCenter entity, CancellationToken cancellationToken = default)
    {
        // Если нет, то вставляем
        var existingServiceCompany = await GetByIdAsync(entity.Id, cancellationToken);
        if (existingServiceCompany == null)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            return;
        }

        var updated = await DbSet
            .Where(x => x.Id == entity.Id && x.UpdatedAt < entity.UpdatedAt)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.UpdatedAt, entity.UpdatedAt)
                    .SetProperty(x => x.ExternalId, entity.ExternalId)
                    .SetProperty(x => x.Name, entity.Name)
                    .SetProperty(x => x.ServiceCompanySapId, entity.ServiceCompanySapId),

                cancellationToken);

        if (updated == 0 && _logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ServiceCenter: {Id} was not updated. Has newer version in DB.", entity.Id);

    }
}
