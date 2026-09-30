using System.Linq.Expressions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

/// <summary>
/// Репозиторий для базовых манипуляций с сущностями.
/// </summary>
/// <typeparam name="TEntity">Тип сущности BaseEntity.</typeparam>
/// /// <typeparam name="TKey">Тип ключа BaseEntity.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : DomainEntity<TKey> where TKey: notnull
{
    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    /// DbSet для работы с сущностью.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    protected Repository(DbContext dbContext)
    {
        Context = dbContext;
        DbSet = Context.Set<TEntity>();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await DbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var existingEntity = await GetByIdAsync(entity.Id, cancellationToken);
        if (existingEntity is null)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            return;
        }

        Context.Entry(existingEntity).CurrentValues.SetValues(entity);
    }

    /// <inheritdoc/>
    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbSet.Remove(entity);
    }

    protected IQueryable<TEntity> Query()
    {
        return DbSet.AsQueryable();
    }

    protected IQueryable<TEntity> QueryWithIncludes(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = DbSet.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        return query;
    }
}
