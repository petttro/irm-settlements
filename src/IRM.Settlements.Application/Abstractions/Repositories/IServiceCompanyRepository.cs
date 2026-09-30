using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IServiceCompanyRepository : IRepository<ServiceCompany, int>
{
    Task<List<ServiceCompany>> SearchAsync(string? search, int limit, CancellationToken cancellationToken = default);

    Task<ServiceCompany?> GetBySapIdAsync(string sapId, CancellationToken cancellationToken);
}
