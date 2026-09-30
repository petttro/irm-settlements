using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class FakeIrmCatalogService : IIrmCatalogService
{
    public Task<ServiceCompanyDetails> GetServiceCompanyDetailsAsync(
        string serviceCompanySapId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateFakeServiceCompanyDetails(serviceCompanySapId));
    }

    public Task<List<ServiceCompanyDetails>> GetServiceCompaniesDetailsAsync(
        List<string> serviceCompanySapIds, CancellationToken cancellationToken = default)
    {
        var result = serviceCompanySapIds
            .Select(serviceCompanySapId => CreateFakeServiceCompanyDetails(serviceCompanySapId))
            .ToList();

        return Task.FromResult(result);
    }

    private ServiceCompanyDetails CreateFakeServiceCompanyDetails(string serviceCompanySapId)
    {
        return new ServiceCompanyDetails
        {
            SapId = serviceCompanySapId,
            Name = "Сервисная компания" + serviceCompanySapId,
            SapContractNumber = "sap-contract-number-" + serviceCompanySapId,
            ExternalContractNumber = "contract-number-" + serviceCompanySapId,
            ContractDate = DateOnly.FromDateTime(DateTime.Now),
            BankAccount = "bank-account-" + serviceCompanySapId,
            BankBic = "bank-bic-" + serviceCompanySapId,
            BankName = "bank-name-" + serviceCompanySapId,
            BankSwiftCode = "bank-swift-code-" + serviceCompanySapId,
            CorrespondentAccount = "correspondent-account-" + serviceCompanySapId,
            Inn = "inn-" + serviceCompanySapId,
            Kpp = "kpp-" + serviceCompanySapId,
        };
    }
}
