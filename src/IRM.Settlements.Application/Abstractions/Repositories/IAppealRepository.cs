using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IAppealRepository : IRepository<Appeal, int>
{
    Task SetReportIdsAsync(HashSet<string> couponNumbers, Guid? reportId, CancellationToken cancellationToken);

    Task<List<Appeal>> GetListAsync(AppealsFilter filter, CancellationToken cancellationToken);

    Task<IEnumerable<Appeal>> GetAllAsync(CancellationToken cancellationToken = default);
}
