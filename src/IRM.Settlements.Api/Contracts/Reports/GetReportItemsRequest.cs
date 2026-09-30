using IRM.Settlements.Api.Contracts.Common;

namespace IRM.Settlements.Api.Contracts.Reports;

public class GetReportItemsRequest : IPagedRequest, ISortableRequest
{
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
    public string? Sort { get; init; }
}
