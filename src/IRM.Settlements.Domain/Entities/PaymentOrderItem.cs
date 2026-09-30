namespace IRM.Settlements.Domain.Entities;

public class PaymentOrderItem
{
    public required string ServiceCompanySapId { get; set; }
    public required string ShopName { get; set; }
    public required string MaterialNumber { get; set; }
    public decimal TotalCost { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public string? Mvz { get; set; }
    public string? SapContractNumber { get; set; }
    public required string ReportNumber { get; set; }

    public static string MvzType => "К";
    public static string Amount => "1";
    public static string Bei => "ЕР";
}
