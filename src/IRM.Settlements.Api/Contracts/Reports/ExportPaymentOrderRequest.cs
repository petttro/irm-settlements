namespace IRM.Settlements.Api.Contracts.Reports;

public record ExportPaymentOrderRequest
{
    public List<Guid> ReportIds { get; init; } = [];
}
