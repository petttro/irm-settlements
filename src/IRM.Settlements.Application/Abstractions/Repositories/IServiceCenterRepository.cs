using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IServiceCenterRepository : IRepository<ServiceCenter, int>
{
    Task<List<ServiceCenter>> SearchAsync(
        string? search, string serviceCompanySapId, int limit, CancellationToken cancellationToken = default);
}
