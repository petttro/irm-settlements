namespace IRM.Settlements.Api.Contracts.Reports;

public record CreateReportRequest
{
    public string? ServiceCompanySapId { get; init; }

    public DateOnly? ServiceDateFrom { get; init; }

    public DateOnly? ServiceDateTo { get; init; }

    public string[]? ServiceCenters { get; init; }

    public string? ReportName { get; init; }
}
