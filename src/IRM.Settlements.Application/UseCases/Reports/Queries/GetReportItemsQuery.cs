using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Queries;

public record GetReportItemsQuery(
    Guid ReportId,
    Paging Paging,
    IReadOnlyList<SortItem> SortItems)
{
    public static class GetReportItemsQueryHandler
    {
        public static async Task<ReportItemsResult> Handle(
            GetReportItemsQuery query,
            IPermissionsService permissionsService,
            IReportRepository reportReadonlyRepository,
            CancellationToken ct)
        {
            var report = await reportReadonlyRepository.GetReportByIdAsync(query.ReportId, ct);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportRead, report);

            var reportItems = await reportReadonlyRepository.GetReportItemsAsync(query.ReportId, query.Paging, query.SortItems, ct);
            var reportItemsResult = reportItems.Select(ReportItemResult.FromEntity).ToList();
            var reportItemsCount = await reportReadonlyRepository.CountReportItemsAsync(query.ReportId, ct);

            return new ReportItemsResult
            {
                TotalSize = reportItemsCount,
                PaginatedQuery = new PaginatedQuery
                {
                    PageIndex = query.Paging.Page,
                    PageSize = query.Paging.PageSize,
                },
                Data = reportItemsResult
            };
        }
    }
}
