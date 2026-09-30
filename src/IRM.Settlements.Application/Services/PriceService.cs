using IRM.Settlements.Application.Abstractions.SAP;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Integrations.SAP;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Services;

public class PriceService : IPriceService
{
    private readonly ISapIntegrationService _sapIntegrationService;

    public PriceService(ISapIntegrationService sapIntegrationService)
    {
        _sapIntegrationService = sapIntegrationService;
    }

    public async Task<IReadOnlyCollection<PriceInfo>> GetPricesAsync(IReadOnlyCollection<ReportItem> reportItems,
        CancellationToken cancellationToken = default)
    {
        var requests = new List<SapPriceRequest>();
        var distinctPriceKeys = new HashSet<(DateOnly ServiceDate, string ServiceCompanySapId, string WareCode, string ShopName)>();

        foreach (var item in reportItems)
        {
            var wareCodes = item.GetWareCodes();
            foreach (var wareCode in wareCodes)
            {
                var key = (item.ServiceDate, item.ServiceCompanySapId, wareCode, item.ShopName);
                if (!distinctPriceKeys.Add(key))
                    continue;

                requests.Add(new SapPriceRequest
                {
                    PriceDate = item.ServiceDate,
                    ServiceCompanySapId = item.ServiceCompanySapId,
                    ShopName = item.ShopName,
                    WareCode = wareCode
                });
            }
        }

        var prices = await _sapIntegrationService.GetPricesAsync(requests, cancellationToken);

        return prices.Select(price => price.ToEntity()).ToList();
    }
}
