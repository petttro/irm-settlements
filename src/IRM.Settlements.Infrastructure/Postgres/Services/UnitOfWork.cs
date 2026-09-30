using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;

namespace IRM.Settlements.Infrastructure.Postgres.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly SettlementsDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(SettlementsDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // 1. сохраняем изменения
        await _dbContext.SaveChangesAsync(cancellationToken);
        // 2. рассылаем доменные события
        await _domainEventDispatcher.DispatchAsync();

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await action();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _domainEventDispatcher.DispatchAsync();
        await transaction.CommitAsync(cancellationToken);
    }
}
