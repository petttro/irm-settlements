namespace IRM.Settlements.Domain.Entities;

public class AdditionalService
{
    public required string Name { get; set; }
    public required string WareCode { get; set; }
    public decimal Cost { get; set; }
    public decimal? SapPrice { get; set; }
    public string? SapNds { get; set; }

    public void SetPrice(decimal price, string nds)
    {
        SapPrice = price;
        SapNds = nds;
    }
}
