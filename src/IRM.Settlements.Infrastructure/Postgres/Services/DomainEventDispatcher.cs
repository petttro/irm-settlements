using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Domain.Entities.Interfaces;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Wolverine;

namespace IRM.Settlements.Infrastructure.Postgres.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly SettlementsDbContext _dbContext;
    private readonly IMessageBus _bus;

    public DomainEventDispatcher(SettlementsDbContext dbContext, IMessageBus bus)
    {
        _dbContext = dbContext;
        _bus = bus;
    }

    public async Task DispatchAsync()
    {
        // достаём domain events
        var events = _dbContext.ChangeTracker
            .Entries<IDomainEntity>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // публикуем
        foreach (var @event in events)
        {
            await _bus.PublishAsync(@event);
        }

        // очищаем
        foreach (var entity in _dbContext.ChangeTracker
                     .Entries<IDomainEntity>()
                     .Select(x => x.Entity))
        {
            entity.ClearDomainEvents();
        }
    }
}
