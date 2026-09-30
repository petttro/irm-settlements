namespace IRM.Settlements.Domain.Entities.Interfaces;

/// <summary>
/// Базовый класс для сущности базы данных.
/// </summary>
public interface IDomainEntity
{
    IReadOnlyCollection<object> DomainEvents { get; }

    void ClearDomainEvents();
}
