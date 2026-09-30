namespace IRM.Settlements.Application.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
