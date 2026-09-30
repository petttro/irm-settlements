namespace IRM.Settlements.Infrastructure.PaymentGateway.Contracts;

public record GetPaymentRequest
{
    /// <summary>
    /// Id платежа
    /// </summary>
    public Guid PaymentId { get; init; }

    /// <summary>
    /// Id мерчанта
    /// </summary>
    public int MerchantId { get; init; }
}
