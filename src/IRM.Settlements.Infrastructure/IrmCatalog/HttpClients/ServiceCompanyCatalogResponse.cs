using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;

public record ServiceCompanyCatalogResponse
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

    public ServiceCompanyDetails ToEntity()
    {
        return new ServiceCompanyDetails
        {
            SapId = SapId,
            Name = Name,

            SapContractNumber = SapContractNumber,
            ExternalContractNumber = ExternalContractNumber,
            ContractDate = ContractDate,

            BankName = BankName,
            BankBic = BankBic,
            BankSwiftCode = BankSwiftCode,
            CorrespondentAccount = CorrespondentAccount,
            BankAccount = BankAccount,
            Inn = Inn,
            Kpp = Kpp
        };
    }
}

public record ServiceCompanyCatalogQueryResponse
{
    public List<ServiceCompanyCatalogResponse> Data { get; init; } = [];
}
