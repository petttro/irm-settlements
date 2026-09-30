using IRM.Settlements.Api.Contracts.Common;

namespace IRM.Settlements.Api.Contracts.Reports;

public record GetReportsRequest: IPagedRequest, ISortableRequest
{
    public string? ServiceCompanyId { get; init; }

    /// <summary>
    /// Поиск по CouponNumber, SaleOrderNumber, OrderNumber
    /// </summary>
    public string? Search { get; init; }
    public string? Status { get; init; }

    public DateTime? CreatedDateFrom { get; init; }
    public DateTime? CreatedDateTo { get; init; }

    public DateTime? SentToPaymentDateFrom { get; init; }
    public DateTime? SentToPaymentDateTo { get; init; }

    public DateTime? PaymentDateFrom { get; init; }
    public DateTime? PaymentDateTo { get; init; }

    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
    public string? Sort { get; init; }
}
