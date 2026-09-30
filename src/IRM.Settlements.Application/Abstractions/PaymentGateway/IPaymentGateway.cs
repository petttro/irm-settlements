using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Abstractions.PaymentGateway;

public interface IPaymentGateway
{
    Task<Guid> CreatePaymentAsync(PaymentInfo paymentInfo, CancellationToken cancellationToken = default);

    Task<List<PaymentStatusInfo>> GetPaymentStatusesAsync(List<Guid> paymentIds, CancellationToken cancellationToken = default);
}
