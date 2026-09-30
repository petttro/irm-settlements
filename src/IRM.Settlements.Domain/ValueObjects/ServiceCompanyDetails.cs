namespace IRM.Settlements.Domain.ValueObjects;

public record ServiceCompanyDetails
{
    public required string SapId { get; init; }
    public required string Name { get; init; }

    public string? SapContractNumber { get; init; }
    public string? ExternalContractNumber { get; init; }
    public DateOnly? ContractDate { get; init; }

    public string? BankName { get; init; }
    public string? BankBic { get; init; }
    public string? BankSwiftCode { get; init; }
    public string? CorrespondentAccount { get; init; }
    public string? BankAccount { get; init; }
    public string? Inn { get; init; }
    public string? Kpp { get; init; }
}
