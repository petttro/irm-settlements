using IRM.Settlements.Api.Contracts.Common;

namespace IRM.Settlements.Api.Contracts.Dictionaries;

public record GetMvzListRequest : IPagedRequest
{
    public string? Search { get; init; }
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
}
