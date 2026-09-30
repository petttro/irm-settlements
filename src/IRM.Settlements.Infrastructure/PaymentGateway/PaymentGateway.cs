using IRM.Settlements.Application.Abstractions.PaymentGateway;
using IRM.Settlements.Domain.ValueObjects;
using IRM.Settlements.Infrastructure.PaymentGateway.Contracts;
using IRM.Settlements.Infrastructure.PaymentGateway.HttpClients;
using IRM.Settlements.Infrastructure.PaymentGateway.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.PaymentGateway;

public class PaymentGateway : IPaymentGateway
{
    private readonly PaymentOrderClient _paymentOrderClient;
    private readonly PaymentStatusClient _paymentStatusClient;
    private readonly ILogger<PaymentGateway> _logger;
    private readonly Finance3PSettings _settings;

    private const int ServicePaymentTypeId = 3;
    private const int PaymentStatusesBatchSize = 500;

    public PaymentGateway(
        PaymentOrderClient paymentOrderClient,
        PaymentStatusClient paymentStatusClient,
        IOptions<Finance3PSettings> options,
        ILogger<PaymentGateway> logger)
    {
        _paymentOrderClient = paymentOrderClient;
        _paymentStatusClient = paymentStatusClient;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task<Guid> CreatePaymentAsync(PaymentInfo paymentInfo, CancellationToken cancellationToken = default)
    {
        var paymentRequest = new SendPaymentRequest
        {
            PaymentId = paymentInfo.PaymentId,
            MerchantId = _settings.MerchantId,
            PaymentTypeId = ServicePaymentTypeId,
            DocAmount = paymentInfo.TotalCost,
            CurrencyCode = _settings.PaymentCurrencyCode,
            PayerInn = _settings.PayerInn,
            PayerKpp = _settings.PayerKpp,
            RecipientBankName = paymentInfo.RecipientBankName,
            RecipientBankBic = paymentInfo.RecipientBankBic,
            RecipientBankSwiftCode = paymentInfo.RecipientBankSwiftCode,
            RecipientCorrespondentAccount = paymentInfo.RecipientCorrespondentAccount,
            RecipientName = paymentInfo.RecipientName,
            RecipientAccount = paymentInfo.RecipientAccount,
            RecipientInn = paymentInfo.RecipientInn,
            RecipientKpp = paymentInfo.RecipientKpp,
            PaymentDescription = $"Оплата услуг, выплата от {DateTime.UtcNow:dd.MM.yyyy}, {paymentInfo.PaymentId}"
        };

        await _paymentOrderClient.SendPaymentsAsync([paymentRequest], cancellationToken);

        return paymentRequest.PaymentId;
    }

    public async Task<List<PaymentStatusInfo>> GetPaymentStatusesAsync(List<Guid> paymentIds, CancellationToken cancellationToken = default)
    {
        paymentIds = paymentIds.Distinct().ToList();

        var statuses = await _paymentStatusClient.GetDictionaryAsync(cancellationToken);
        var statusMap = statuses.ToDictionary(s => s.Id);

        var result = new List<PaymentStatusInfo>();

        foreach (var chunk in paymentIds.Chunk(PaymentStatusesBatchSize))
        {
            var request = chunk
                .Select(paymentId => new GetPaymentRequest
                {
                    PaymentId = paymentId,
                    MerchantId = _settings.MerchantId
                })
                .ToList();

            var response = await _paymentStatusClient.GetPaymentsArrayAsync(request, cancellationToken);

            foreach (var payment in response)
            {
                if (!statusMap.TryGetValue(payment.LastStatusId, out var status))
                {
                    _logger.LogWarning("Payment gateway returned an unexpected status {Status}.", payment.LastStatusId);
                    continue;
                }

                result.Add(new PaymentStatusInfo
                {
                    PaymentId = payment.PaymentId,
                    StatusId = status.Id,
                    StatusName = status.Name,
                    IsFinal = status.IsFinal,
                    IsPositive = status.IsPositive
                });
            }
        }

        return result;
    }
}
