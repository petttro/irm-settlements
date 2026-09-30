namespace IRM.Settlements.Application.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync();
}
