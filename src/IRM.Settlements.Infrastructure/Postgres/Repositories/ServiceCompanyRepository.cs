using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

/// <summary>
/// Репозиторий для работы с сущностью ServiceCompany.
/// </summary>
public sealed class ServiceCompanyRepository : Repository<ServiceCompany, int>, IServiceCompanyRepository
{
    private readonly ILogger<ServiceCompanyRepository> _logger;

    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных Settlement.</param>
    /// <param name="logger">Логгер</param>
    public ServiceCompanyRepository(SettlementsDbContext context, ILogger<ServiceCompanyRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public async Task<List<ServiceCompany>> SearchAsync(string? search, int limit, CancellationToken cancellationToken = default)
    {
        var query = Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(sc => EF.Functions.ILike(sc.Name, $"%{search}%"));

        return await query
            .OrderBy(sc => sc.Name)
            .Take(limit).ToListAsync(cancellationToken);
    }

    public async Task<ServiceCompany?> GetBySapIdAsync(string sapId, CancellationToken cancellationToken)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(sc => sc.SapId == sapId, cancellationToken);
    }

    public override async Task UpsertAsync(ServiceCompany entity, CancellationToken cancellationToken = default)
    {
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
                    .SetProperty(x => x.SapId, entity.SapId)
                    .SetProperty(x => x.Name, entity.Name),
                cancellationToken);

        if (updated == 0 && _logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ServiceCompany: {Id} was not updated. Has newer version in DB.", entity.Id);
    }
}
