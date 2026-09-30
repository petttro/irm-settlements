namespace IRM.Settlements.Api.Contracts.Reports;

public record ExportReportsRequest
{
    public string? ServiceCompanyId { get; init; }

    public string? Status { get; init; }

    public DateTime? CreatedDateFrom { get; init; }
    public DateTime? CreatedDateTo { get; init; }

    public DateTime? SentToPaymentDateFrom { get; init; }
    public DateTime? SentToPaymentDateTo { get; init; }

    public DateTime? PaymentDateFrom { get; init; }
    public DateTime? PaymentDateTo { get; init; }
}
