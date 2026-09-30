namespace IRM.Settlements.Domain.ValueObjects;

public class ReportPaymentOrderInfo
{
    public required string ServiceCompanySapId { get; set; }
    public required string ShopName { get; set; }
    public string? SapNds { get; set; }
    public required decimal TotalCost { get; set; }
    public required string ReportNumber { get; set; }
    public string? MvzCode { get; set; }
}
