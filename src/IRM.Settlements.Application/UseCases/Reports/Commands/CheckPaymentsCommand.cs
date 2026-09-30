using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.PaymentGateway;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.UseCases.Reports.Commands;

public record CheckPaymentsCommand
{
    public static class CheckPaymentsCommandHandler
    {
        public static async Task Handle(
            CheckPaymentsCommand command,
            IReportRepository reportRepository,
            IPaymentGateway paymentGateway,
            IUnitOfWork uow,
            CancellationToken cancellationToken)
        {
            var filter = new ReportsFilter { Status = ReportStatus.SentToPayment };
            var reports = await reportRepository.ListAsync(filter, paging: null, sortItems: [], cancellationToken);
            var reportsMap = reports.Where(r => r.PaymentId.HasValue).ToDictionary(r => r.PaymentId!.Value);

            var paymentIds = reportsMap.Keys.ToList();
            if (paymentIds.Count == 0)
                return;

            var paymentStatuses = await paymentGateway.GetPaymentStatusesAsync(paymentIds, cancellationToken);
            foreach (var paymentStatus in paymentStatuses)
            {
                if (!paymentStatus.IsFinal)
                    continue;

                if (paymentStatus.IsPositive != true)
                    continue;

                if (!reportsMap.TryGetValue(paymentStatus.PaymentId, out var report))
                    throw new InvalidOperationException($"PaymentId {paymentStatus.PaymentId} does not exist");

                report.MarkAsPaid();
                report.UpdatedAt = DateTime.UtcNow;
                report.UpdatedBy = "System";
            }

            await uow.CommitAsync(cancellationToken);
        }
    }
}
