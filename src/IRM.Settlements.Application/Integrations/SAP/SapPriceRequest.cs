namespace IRM.Settlements.Application.Integrations.SAP;

public record SapPriceRequest
{
    public required DateOnly PriceDate { get; init; }
    public required string ServiceCompanySapId { get; init; }
    public required string ShopName { get; init; }
    public required string WareCode { get; init; }
}
