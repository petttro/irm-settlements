namespace IRM.Settlements.Domain.ValueObjects;

public record PriceInfo
{
    public required string ServiceCompanySapId { get; init; }
    public required string WareCode { get; init; }
    public required string ShopName { get; init; }
    public required DateOnly ServiceDate { get; init; }

    public required decimal Price { get; init; }
    public required string Nds { get; init; }
}
