namespace IRM.Settlements.Api.Contracts.Common;

public interface ISortableRequest
{
    public string? Sort { get; init; }
}