namespace IRM.Settlements.Api.Contracts.Common;

public interface IPagedRequest
{
    public int? PageIndex { get; init; }

    public int? PageSize { get; init; }
}
