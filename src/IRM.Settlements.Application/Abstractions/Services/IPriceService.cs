using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Abstractions.Services;

public interface IPriceService
{
    Task<IReadOnlyCollection<PriceInfo>> GetPricesAsync(IReadOnlyCollection<ReportItem> reportItems,
        CancellationToken cancellationToken = default);
}
