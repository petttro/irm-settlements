namespace IRM.Settlements.Api.Contracts.Reports;

public record UpdateReportRequest
{
    public required DateOnly ServiceDateFrom { get; init; }

    public required DateOnly ServiceDateTo { get; init; }

    public string[]? ServiceCenters { get; init; }

    public string? ReportName { get; init; }
}
