using IRM.Settlements.Application.Integrations.SAP;

namespace IRM.Settlements.Application.Abstractions.SAP;

public interface ISapIntegrationService
{
    Task<IReadOnlyCollection<SapPriceResponse>> GetPricesAsync(
        IReadOnlyCollection<SapPriceRequest> rows, CancellationToken cancellationToken = default);
}
