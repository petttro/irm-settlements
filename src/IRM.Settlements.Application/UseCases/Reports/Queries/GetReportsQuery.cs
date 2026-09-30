using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Queries;

public record GetReportsQuery(
    string? ServiceCompanySapId,
    string? Search,
    ReportStatus? Status,
    DateTimeRange CreatedAt,
    DateTimeRange SentToPaymentDate,
    DateTimeRange PaymentDate,
    Paging Paging,
    List<SortItem> SortItems) : IServiceCompanyResource
{
    public static class GetReportsQueryHandler
    {
        public static async Task<GridResult<ReportResult>> Handle(
            GetReportsQuery query,
            IReportRepository reportRepository,
            IPermissionsService permissionsService,
            CancellationToken cancellationToken)
        {
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportRead, query);

            var filter = new ReportsFilter(query);
            var reports = await reportRepository.ReadOnlyListAsync(filter, query.Paging, query.SortItems, cancellationToken);
            var reportsCount = await reportRepository.CountAsync(filter, cancellationToken);

            var result = new GridResult<ReportResult>
            {
                TotalSize = reportsCount,
                PaginatedQuery = new PaginatedQuery
                {
                    PageIndex = query.Paging.Page,
                    PageSize = query.Paging.PageSize,
                },
                Data = reports
                    .Select(report => ReportResult.FromEntity(report, permissionsService.GetReportPermissions(report)))
                    .ToList()
            };

            return result;
        }
    }
}
