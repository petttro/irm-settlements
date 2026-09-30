using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Queries;

public record GetReportQuery(Guid ReportId)
{
    public static class GetReportQueryHandler
    {
        public static async Task<ReportResult> Handle(
            GetReportQuery query,
            IPermissionsService permissionsService,
            IReportRepository reportReadonlyRepository,
            CancellationToken ct)
        {
            var report = await reportReadonlyRepository.GetReportByIdAsync(query.ReportId, ct);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportRead, report);

            var permissions = permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
