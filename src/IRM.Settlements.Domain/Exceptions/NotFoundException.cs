namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Базовое исключение, возникающее при отсутствии какой-либо сущности.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="NotFoundException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Проверка значения на null с выбросом <see cref="NotFoundException"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="message">Сообщение.</param>
    /// <exception cref="NotFoundException">Выбрасывается, если значение <paramref name="message"/> равно null.</exception>
    public static void ThrowIfNull(object? value, string message)
    {
        if (value is not null)
            return;

        throw new NotFoundException(message);
    }
}
