using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record DeleteReportCommand(Guid ReportId)
{
    public static class DeleteReportCommandHandler
    {
        public static async Task Handle(
            DeleteReportCommand command,
            IPermissionsService permissionsService,
            IReportRepository reportRepository,
            IAppealRepository appealRepository,
            IUnitOfWork uow,
            CancellationToken ct)
        {
            var report = await reportRepository.GetReportWithItemsAsync(command.ReportId, ct);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportDelete, report);

            // Помечаем удаленные ЗНУ свободными от отчета
            var couponNumbers = report.Items.Select(item => item.CouponNumber).ToHashSet();
            await appealRepository.SetReportIdsAsync(couponNumbers, null, ct);

            report.Delete();
            await uow.CommitAsync(ct);

            reportRepository.Remove(report);
            await uow.CommitAsync(ct);
        }
    }
}
