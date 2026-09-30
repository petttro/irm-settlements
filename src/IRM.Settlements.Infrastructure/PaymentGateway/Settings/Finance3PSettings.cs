namespace IRM.Settlements.Infrastructure.PaymentGateway.Settings;

public class Finance3PSettings
{
    public const string SectionName =  "Finance3P";

    public required string PaymentOrderUrl { get; set; }

    public required string PaymentStatusUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int MerchantId { get; set; }

    public string PayerInn { get; set; } = "7707548740";

    public string PayerKpp { get; set; } = "997350001";

    public string PaymentCurrencyCode { get; set; } = "810";

    public int CheckPaymentStatusIntervalSeconds { get; set; } = 60;
}
