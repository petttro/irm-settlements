using IRM.Settlements.Domain.Entities.Interfaces;

namespace IRM.Settlements.Application.Abstractions.Repositories;

/// <summary>
/// Репозиторий для базовых манипуляций с сущностями.
/// </summary>
/// <typeparam name="TEntity">Сущность типа Entity</typeparam>
/// <typeparam name="TKey">Тип ключа типа Entity</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : IDomainEntity
{
    /// <summary>
    /// Получить сущность по ID.
    /// </summary>
    /// <param name="id">ID сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сущность или null, если не найдена.</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую сущность.
    /// </summary>
    /// <param name="entity">Сущность для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить или обновить существующую сущность
    /// </summary>
    /// <param name="entity">Сущность для обновления.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сущность.
    /// </summary>
    /// <param name="entity">Сущность для удаления.</param>
    void Remove(TEntity entity);
}
