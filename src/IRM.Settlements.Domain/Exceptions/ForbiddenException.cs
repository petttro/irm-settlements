namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее если запрещенно какое-либо действие.
/// </summary>
public class ForbiddenException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ForbiddenException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    public ForbiddenException(string message) : base(message)
    {
    }
}
