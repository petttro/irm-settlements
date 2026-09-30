using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record UpdateReportCommand(
    Guid ReportId,
    string? ReportName,
    List<string> ServiceCenterExternalIds,
    DateOnlyRange ServiceDate)
{
    public class UpdateReportCommandHandler
    {
        private readonly IPermissionsService _permissionsService;
        private readonly IPriceService _priceService;
        private readonly IUserContext _userContext;
        private readonly IAppealRepository _appealRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IUnitOfWork _uow;

        public UpdateReportCommandHandler(
            IPermissionsService permissionsService,
            IPriceService priceService,
            IUserContext userContext,
            IAppealRepository appealRepository,
            IReportRepository reportRepository,
            IUnitOfWork uow)
        {
            _permissionsService = permissionsService;
            _priceService = priceService;
            _userContext = userContext;
            _appealRepository = appealRepository;
            _reportRepository = reportRepository;
            _uow = uow;
        }

        public async Task<ReportResult> Handle(UpdateReportCommand command, CancellationToken cancellationToken)
        {
            var report = await _reportRepository.GetReportWithItemsAsync(command.ReportId, cancellationToken);
            _permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportWrite, report);

            var filter = new AppealsFilter
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                ServiceDate = command.ServiceDate,
                ServiceCenterExternalIds = command.ServiceCenterExternalIds
            };
            var newAppeals = await _appealRepository.GetListAsync(filter, cancellationToken);

            if (command.ReportName != null)
                report.Name = command.ReportName;

            report.ServiceDateFrom = command.ServiceDate.From!.Value;
            report.ServiceDateTo = command.ServiceDate.To!.Value;
            report.ServiceCenterExternalIds = command.ServiceCenterExternalIds;
            report.UpdatedBy = _userContext.UserName;

            // Делаем дополнительную фильтрацию тут - newAppeals не содержит ЗНУ уже добавленные в отчет
            // Позиции за пределами нового диапазона дат
            var removedCouponNumbers = report.Items
                .Where(item =>
                {
                    var outsideDateRange =
                        item.ServiceDate < command.ServiceDate.From ||
                        item.ServiceDate > command.ServiceDate.To;

                    var outsideServiceCenterFilter =
                        command.ServiceCenterExternalIds.Count != 0 &&
                        !command.ServiceCenterExternalIds.Contains(item.ServiceCenterExternalId);

                    return outsideDateRange || outsideServiceCenterFilter;
                })
                .Select(item => item.CouponNumber)
                .ToHashSet();

            if (newAppeals.Count > 0)
            {
                var newReportItems = newAppeals.Select(ReportItem.FromAppeal).ToList();
                report.AddItems(newReportItems);

                var newCouponNumbers = newReportItems.Select(x => x.CouponNumber).ToHashSet();
                await _appealRepository.SetReportIdsAsync(newCouponNumbers, report.Id, cancellationToken);

                var prices = await _priceService.GetPricesAsync(report.Items, cancellationToken);
                report.ApplyPrices(prices);
            }

            if (removedCouponNumbers.Count > 0)
            {
                report.RemoveItems(removedCouponNumbers);
                await _appealRepository.SetReportIdsAsync(removedCouponNumbers, null, cancellationToken);
            }

            await _uow.CommitAsync(cancellationToken);

            var permissions = _permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
