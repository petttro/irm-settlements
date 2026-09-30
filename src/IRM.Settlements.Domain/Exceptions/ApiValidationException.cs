namespace IRM.Settlements.Domain.Exceptions;

public class ApiValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ApiValidationException(Dictionary<string, string[]> errors)
        : base("Возникли ошибки валидации")
    {
        Errors = errors;
    }
}
