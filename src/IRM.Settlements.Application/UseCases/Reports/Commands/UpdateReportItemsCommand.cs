using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record UpdateReportItemsCommand(Guid ReportId, List<ReportItemOperation> Operations)
{
    public static class UpdateReportQueryHandler
    {
        public static async Task<ReportResult> Handle(
            UpdateReportItemsCommand command,
            IPermissionsService permissionsService,
            IUserContext userContext,
            IReportRepository reportRepository,
            IAppealRepository appealRepository,
            IUnitOfWork uow,
            CancellationToken ct)
        {
            var report = await reportRepository.GetReportWithItemsAsync(command.ReportId, ct);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportWrite, report);

            // Delete report items
            var deletedCouponNumbers = command.Operations
                .Where(o => o.Type == Constants.OperationTypes.Delete)
                .Select(o => o.CouponNumber)
                .ToHashSet();

            if (deletedCouponNumbers.Count > 0)
            {
                report.RemoveItems(deletedCouponNumbers);
                await appealRepository.SetReportIdsAsync(deletedCouponNumbers, null, ct);
            }

            report.UpdatedBy = userContext.UserName;
            report.UpdatedAt = DateTime.UtcNow;

            await uow.CommitAsync(ct);

            var permissions = permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}

public class ReportItemOperation
{
    public string Type { get; set; } = default!;
    public required string CouponNumber { get; set; }
}
