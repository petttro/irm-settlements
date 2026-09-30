using FluentValidation;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Application.Abstractions.PaymentGateway;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Application.Validators;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;
using IRM.Settlements.Domain.ValueObjects;
using Microsoft.FeatureManagement;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record SendToPaymentCommand(Guid ReportId)
{
    public class SendToPaymentCommandHandler
    {
        private const string PaymentGatewayFeatureFlag = "UsePaymentGateway";

        private readonly IPermissionsService _permissionsService;
        private readonly IReportRepository _reportRepository;
        private readonly IIrmCatalogService _irmCatalogService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _uow;
        private readonly IFeatureManager _featureManager;

        public SendToPaymentCommandHandler(
            IPermissionsService permissionsService,
            IReportRepository reportRepository,
            IIrmCatalogService irmCatalogService,
            IPaymentGateway paymentGateway,
            IUserContext userContext,
            IUnitOfWork uow,
            IFeatureManager featureManager)
        {
            _permissionsService = permissionsService;
            _reportRepository = reportRepository;
            _irmCatalogService = irmCatalogService;
            _paymentGateway = paymentGateway;
            _userContext = userContext;
            _uow = uow;
            _featureManager = featureManager;
        }

        public async Task<ReportResult> Handle(SendToPaymentCommand command, CancellationToken cancellationToken)
        {
            // Здесь важно доставать отчет с Items, чтобы проверить позиции на нулевые суммы в объекте Report (иначе будет исключение)
            var report = await _reportRepository.GetReportWithItemsAsync(command.ReportId, cancellationToken);
            _permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportSendToPayment, report);

            Guid? paymentId = null;

            // Оплата отчета производится только при переходе статуса отчета из "Черновик" -> "Передан в оплату"
            // возможен кейс, когда переход стауса "Оплачен" -> "Передан в оплату", тогда платежный шлюз не тревожим
            if (report.Status == ReportStatus.Draft && await _featureManager.IsEnabledAsync(PaymentGatewayFeatureFlag))
            {
                var serviceCompanyDetails =
                    await _irmCatalogService.GetServiceCompanyDetailsAsync(report.ServiceCompanySapId, cancellationToken);

                var validationResult = await new ServiceCompanyDetailsValidator().ValidateAsync(serviceCompanyDetails, cancellationToken);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var paymentInfo = new PaymentInfo
                {
                    PaymentId = Guid.NewGuid(),
                    TotalCost = report.TotalCost,
                    RecipientBankName = serviceCompanyDetails.BankName!,
                    RecipientBankBic = serviceCompanyDetails.BankBic,
                    RecipientBankSwiftCode = serviceCompanyDetails.BankSwiftCode,
                    RecipientCorrespondentAccount = serviceCompanyDetails.CorrespondentAccount,
                    RecipientName = serviceCompanyDetails.Name,
                    RecipientAccount = serviceCompanyDetails.BankAccount!,
                    RecipientInn = serviceCompanyDetails.Inn,
                    RecipientKpp = serviceCompanyDetails.Kpp
                };

                paymentId = await _paymentGateway.CreatePaymentAsync(paymentInfo, cancellationToken);
            }

            report.MarkAsSendToPayment(paymentId);
            report.UpdatedAt = DateTime.UtcNow;
            report.UpdatedBy = _userContext.UserName;

            await _uow.CommitAsync(cancellationToken);

            var permissions = _permissionsService.GetReportPermissions(report);
            return ReportResult.FromEntity(report, permissions);
        }
    }
}
