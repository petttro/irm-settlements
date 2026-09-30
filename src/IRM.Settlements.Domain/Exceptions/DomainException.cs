namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее в домене приложения.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DomainException"/>.
    /// </summary>
    protected DomainException()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DomainException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    protected DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DomainException"/> с указанным сообщением об ошибке и ссылкой на внутреннее исключение, которое стало причиной этого исключения.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение.</param>
    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
