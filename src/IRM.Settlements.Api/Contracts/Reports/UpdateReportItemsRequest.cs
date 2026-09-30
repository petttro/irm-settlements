namespace IRM.Settlements.Api.Contracts.Reports;

public record UpdateReportItemsRequest
{
    public List<ReportItemOperationRequest> Operations { get; init; } = [];
}

public record ReportItemOperationRequest
{
    public required string Type { get; init; }
    public required string CouponNumber { get; init; }
}
