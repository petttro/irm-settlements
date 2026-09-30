using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Abstractions.IrmCatalog;

public interface IIrmCatalogService
{
    Task<ServiceCompanyDetails> GetServiceCompanyDetailsAsync(string serviceCompanySapId, CancellationToken cancellationToken = default);

    Task<List<ServiceCompanyDetails>> GetServiceCompaniesDetailsAsync(
        List<string> serviceCompanySapIds, CancellationToken cancellationToken = default);
}
