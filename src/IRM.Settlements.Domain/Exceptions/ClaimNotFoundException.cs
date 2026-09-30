namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Базовое исключение, возникающее при отсутствии необходимого Claim для пользователя.
/// </summary>
public class ClaimNotFoundException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ClaimNotFoundException"/>.
    /// </summary>
    public ClaimNotFoundException()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ClaimNotFoundException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    public ClaimNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ClaimNotFoundException"/> с указанным сообщением об ошибке и ссылкой на внутреннее исключение, которое стало причиной этого исключения.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение.</param>
    public ClaimNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
