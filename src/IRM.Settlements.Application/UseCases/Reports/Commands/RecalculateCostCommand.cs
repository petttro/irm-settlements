using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record RecalculateCostCommand(Guid ReportId)
{
    public static class RecalculateCostCommandHandler
    {
        public static async Task<ReportResult> Handle(
            RecalculateCostCommand command,
            IReportRepository reportRepository,
            IPriceService priceService,
            IUnitOfWork uow,
            IPermissionsService permissionsService,
            CancellationToken cancellationToken)
        {
            var report = await reportRepository.GetReportWithItemsAsync(command.ReportId, cancellationToken);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportRecalculate, report);

            var prices = await priceService.GetPricesAsync(report.Items, cancellationToken);
            report.ApplyPrices(prices);

            await uow.CommitAsync(cancellationToken);

            var permissions = permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
