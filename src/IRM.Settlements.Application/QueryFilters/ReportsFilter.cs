using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.UseCases.Reports.Queries;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.QueryFilters;

public class ReportsFilter
{
    public ReportsFilter() { }

    public ReportsFilter(GetReportsQuery query)
    {
        ServiceCompanySapId = query.ServiceCompanySapId;
        Search  = query.Search;
        Status = query.Status;
        CreatedAt = query.CreatedAt;
        SentToPaymentDate = query.SentToPaymentDate;
        PaymentDate = query.PaymentDate;
    }

    public ReportsFilter(ExportReportsQuery query)
    {
        ServiceCompanySapId = query.ServiceCompanySapId;
        Status = query.Status;
        CreatedAt = query.CreatedAt;
        SentToPaymentDate = query.SentToPaymentDate;
        PaymentDate = query.PaymentDate;
    }

    public string? ServiceCompanySapId { get; init; }
    public string? Search { get; init; }
    public ReportStatus? Status { get; init; }
    public DateTimeRange? CreatedAt { get; init; }
    public DateTimeRange? SentToPaymentDate { get; init; }
    public DateTimeRange? PaymentDate { get; init; }
    public List<Guid> ReportIds { get; init; } = [];
}
