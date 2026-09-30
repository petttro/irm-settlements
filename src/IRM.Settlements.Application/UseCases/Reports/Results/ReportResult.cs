using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.UseCases.Reports.Results;

public class ReportResult : IServiceCompanyResource
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public string? Name { get; set; }
    public required string ServiceCompanySapId { get; set; }
    public required string ServiceCompanyName { get; set; }
    public IEnumerable<string> ServiceCenterExternalIds { get; set; } = [];
    public required DateOnly ServiceDateFrom { get; set; }
    public required DateOnly ServiceDateTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentToPaymentDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public ReportStatus Status { get; set; }
    public decimal TotalCost { get; set; }
    public int ItemsCount { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];

    public static ReportResult FromEntity(Report report, IEnumerable<string> permissions)
    {
        var result = new ReportResult
        {
            Id = report.Id,
            Number = report.Number,
            Name = report.Name,
            ServiceCenterExternalIds = report.ServiceCenterExternalIds.ToArray(),
            ServiceCompanySapId = report.ServiceCompanySapId,
            ServiceCompanyName = report.ServiceCompanyName,
            ServiceDateFrom =  report.ServiceDateFrom,
            ServiceDateTo =  report.ServiceDateTo,
            Status = report.Status,

            CreatedAt = report.CreatedAt,
            SentToPaymentDate = report.SentToPaymentDate,
            PaymentDate = report.PaymentDate,
            ItemsCount = report.ItemsCount,
            TotalCost = report.TotalCost,
            Permissions = permissions
        };

        return result;
    }
}
