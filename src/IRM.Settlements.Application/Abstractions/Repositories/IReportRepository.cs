using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IReportRepository : IRepository<Report, Guid>
{
    Task<Report> GetReportWithItemsAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task<Report> GetReportByIdAsync(Guid reportId, CancellationToken ct);

    Task<List<Report>> ListAsync(ReportsFilter filter, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken);

    Task<List<Report>> ReadOnlyListAsync(ReportsFilter filter, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken);

    Task<List<ReportPaymentOrderInfo>> GetPaymentOrderDataAsync(List<Guid> reportIds, CancellationToken cancellationToken);

    Task<List<ReportItem>> GetReportItemsAsync(Guid reportId, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken);

    Task<int> CountAsync(ReportsFilter filter, CancellationToken cancellationToken);

    Task<int> CountReportItemsAsync(Guid reportId, CancellationToken cancellationToken = default);
}
