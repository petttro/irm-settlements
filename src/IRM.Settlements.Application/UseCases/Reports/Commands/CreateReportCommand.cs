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

public record CreateReportCommand(
    string ServiceCompanySapId,
    List<string> ServiceCenterExternalIds,
    string? ReportName,
    DateOnlyRange ServiceDate) : IServiceCompanyResource
{
    public class CreateReportCommandHandler
    {
        private readonly IUserContext _userContext;
        private readonly IAppealRepository _appealRepository;
        private readonly IReportNumberGenerator _reportNumberGenerator;
        private readonly IReportRepository _reportRepository;
        private readonly IPriceService _priceService;
        private readonly IPermissionsService _permissionsService;
        private readonly IUnitOfWork _uow;

        public CreateReportCommandHandler(
            IUserContext userContext,
            IAppealRepository appealRepository,
            IReportNumberGenerator reportNumberGenerator,
            IReportRepository reportRepository,
            IPriceService priceService,
            IPermissionsService permissionsService,
            IUnitOfWork uow)
        {
            _userContext = userContext;
            _appealRepository = appealRepository;
            _reportNumberGenerator = reportNumberGenerator;
            _reportRepository = reportRepository;
            _priceService = priceService;
            _permissionsService = permissionsService;
            _uow = uow;
        }

        public async Task<ReportResult> Handle(CreateReportCommand command, CancellationToken cancellationToken)
        {
            _permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportCreate, command);

            var filter = new AppealsFilter
            {
                ServiceCompanySapId = command.ServiceCompanySapId,
                ServiceCenterExternalIds = command.ServiceCenterExternalIds,
                ServiceDate = command.ServiceDate
            };

            var appeals = await _appealRepository.GetListAsync(filter, cancellationToken);
            if (appeals.Count == 0)
                throw new ArgumentException("ЗНУ не найдены с заданными параметрами отчета");

            var serviceCompany = appeals[0].ServiceCompany;
            var sequenceNumber = await _reportNumberGenerator.NextAsync(DateTime.UtcNow.Year, cancellationToken);
            var createdAt = DateTime.UtcNow;

            var report = new Report
            {
                Id = Guid.NewGuid(),
                Number = $"IRM{sequenceNumber:D5}/{createdAt.Year}",
                Name = command.ReportName,
                ServiceDateFrom = command.ServiceDate.From!.Value,
                ServiceDateTo = command.ServiceDate.To!.Value,
                ServiceCompanySapId = command.ServiceCompanySapId,
                ServiceCompanyName = serviceCompany.Name,
                ServiceCenterExternalIds = command.ServiceCenterExternalIds,
                CreatedAt = createdAt,
                CreatedBy = _userContext.UserName,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = _userContext.UserName
            };

            report.Create();

            var reportItems = appeals.Select(ReportItem.FromAppeal).ToList();
            report.AddItems(reportItems);

            var prices = await _priceService.GetPricesAsync(report.Items, cancellationToken);
            report.ApplyPrices(prices);

            var couponNumbers = appeals.Select(a => a.CouponNumber).ToHashSet();

            await _uow.ExecuteAsync(async () =>
            {
                await _reportRepository.AddAsync(report, cancellationToken);
                // Помечаем ЗНУ добавленными в отчет
                await _appealRepository.SetReportIdsAsync(couponNumbers, report.Id, cancellationToken);
            }, cancellationToken);

            var permissions = _permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
