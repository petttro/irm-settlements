using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Integrations.SAP;

public record SapPriceResponse
{
    public required DateOnly PriceDate { get; init; }
    public required string ServiceCompanySapId { get; init; }
    public required string ShopName { get; init; }
    public required string WareCode { get; init; }
    public required decimal Price { get; init; }
    public required string Nds { get; init; }

    public PriceInfo ToEntity()
    {
        return new PriceInfo
        {
            ServiceCompanySapId = ServiceCompanySapId,
            WareCode = WareCode,
            ShopName = ShopName,
            ServiceDate = PriceDate,
            Price = Price,
            Nds = Nds
        };
    }
}
