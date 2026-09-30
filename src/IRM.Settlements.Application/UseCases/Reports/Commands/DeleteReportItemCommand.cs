using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record DeleteReportItemCommand(Guid ReportId, Guid ReportItemId)
{
    public class DeleteReportItemCommandHandler
    {
        private readonly IPermissionsService _permissionsService;
        private readonly IUserContext _userContext;
        private readonly IReportRepository _reportRepository;
        private readonly IAppealRepository _appealRepository;
        private readonly IUnitOfWork _uow;

        public DeleteReportItemCommandHandler(
            IPermissionsService permissionsService,
            IUserContext userContext,
            IReportRepository reportRepository,
            IAppealRepository appealRepository,
            IUnitOfWork uow)
        {
            _permissionsService = permissionsService;
            _userContext = userContext;
            _reportRepository = reportRepository;
            _appealRepository = appealRepository;
            _uow = uow;
        }

        public async Task<ReportResult> Handle(DeleteReportItemCommand command, CancellationToken ct)
        {
            var report = await _reportRepository.GetReportWithItemsAsync(command.ReportId, ct);
            _permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportWrite, report);

            var deletingItem = report.Items.FirstOrDefault(i => i.Id == command.ReportItemId);
            if (deletingItem == null)
                throw new NotFoundException("Report item not found");

            report.RemoveItems([deletingItem.CouponNumber]);
            await _appealRepository.SetReportIdsAsync([deletingItem.CouponNumber], null, ct);

            report.UpdatedBy = _userContext.UserName;

            await _uow.CommitAsync(ct);

            var permissions = _permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
