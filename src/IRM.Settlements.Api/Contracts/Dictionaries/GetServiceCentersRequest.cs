namespace IRM.Settlements.Api.Contracts.Dictionaries;

public record GetServiceCentersRequest
{
    public string? Search { get; init; }

    public string? ServiceCompanySapId { get; init; }

    public int? PageSize { get; init; }
}
