namespace IRM.Settlements.Infrastructure.SAP.Exceptions;

public class SapInvalidResponseException : Exception
{
    public SapInvalidResponseException()
    {

    }

    public SapInvalidResponseException(string message) : base(message)
    {

    }
};
