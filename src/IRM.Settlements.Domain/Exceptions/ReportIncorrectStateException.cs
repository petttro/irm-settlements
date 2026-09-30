namespace IRM.Settlements.Domain.Exceptions;

public class ReportIncorrectStateException : DomainException
{
    public ReportIncorrectStateException(string message) : base(message)
    {

    }
}
