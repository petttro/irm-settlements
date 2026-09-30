using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record ConfirmPaymentCommand(Guid ReportId)
{
    public class ConfirmPaymentCommandHandler
    {
        private readonly IPermissionsService _permissionsService;
        private readonly IReportRepository _reportRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _uow;

        public ConfirmPaymentCommandHandler(
            IPermissionsService permissionsService,
            IReportRepository reportRepository,
            IUserContext userContext,
            IUnitOfWork uow)
        {
            _permissionsService = permissionsService;
            _reportRepository = reportRepository;
            _userContext = userContext;
            _uow = uow;
        }

        public async Task<ReportResult> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
        {
            var report = await _reportRepository.GetReportByIdAsync(command.ReportId, cancellationToken);
            _permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportConfirmPayment, report);

            report.MarkAsPaid();
            report.UpdatedAt = DateTime.UtcNow;
            report.UpdatedBy = _userContext.UserName;

            await _uow.CommitAsync(cancellationToken);

            var permissions = _permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
