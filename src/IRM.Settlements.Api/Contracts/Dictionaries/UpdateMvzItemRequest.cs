namespace IRM.Settlements.Api.Contracts.Dictionaries;

public record UpdateMvzItemRequest
{
    public required string MvzCode { get; init; }

    public required string MvzName { get; init; }
}
