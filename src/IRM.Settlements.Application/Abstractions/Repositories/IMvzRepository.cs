using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IMvzRepository : IRepository<MvzItem, string>
{
    Task<List<MvzItem>> SearchAsync(string? search, Paging? paging, CancellationToken cancellationToken = default);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
}
