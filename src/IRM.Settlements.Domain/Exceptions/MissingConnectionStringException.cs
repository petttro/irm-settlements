namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при отсутствии строки подключения к базе данных.
/// </summary>
public class MissingConnectionStringException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MissingConnectionStringException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    public MissingConnectionStringException(string message) : base(message)
    {
    }

    /// <summary>
    /// Проверка значения на null с выбросом <see cref="MissingConnectionStringException"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="settingName">Проверяемое значение.</param>
    /// <exception cref="MissingConnectionStringException">Выбрасывается, если значение <paramref name="settingName"/> равно null.</exception>
    public static void ThrowIfNull(string value, string settingName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        settingName = string.IsNullOrWhiteSpace(settingName)
            ? "UnknownConnectionString"
            : settingName;

        throw new MissingConnectionStringException($"Отсутствует значение для строки подключения \"{settingName}\".");
    }
}
