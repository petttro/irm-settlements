using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.Infrastructure.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

/// <summary>
/// Репозиторий для работы с сущностью МВЗ.
/// </summary>
public sealed class MvzRepository : Repository<MvzItem, string>, IMvzRepository
{
    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных Settlement.</param>
    public MvzRepository(SettlementsDbContext context)
        : base(context)
    {
    }

    public async Task<List<MvzItem>> SearchAsync(string? search, Paging? paging, CancellationToken cancellationToken = default)
    {
        var query = Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(sc => EF.Functions.ILike(sc.Id, $"%{search}%"));

        if (paging != null)
            query = query.ApplyPaging(paging.Page, paging.PageSize);

        return await query
            .OrderBy(sc => sc.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(sc => EF.Functions.ILike(sc.Id, $"%{search}%"));

        return await query.CountAsync(cancellationToken);
    }

    public override async Task UpsertAsync(MvzItem entity, CancellationToken cancellationToken = default)
    {
        var existingMvzItem = await GetByIdAsync(entity.Id, cancellationToken);
        if (existingMvzItem == null)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }
    }
}
