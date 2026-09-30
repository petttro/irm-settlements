namespace IRM.Settlements.Infrastructure.SAP.Settings;

public sealed record SapPriceSettings
{
    public static string SectionName { get; set; } = nameof(SapPriceSettings);

    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string Url { get; init; }
}
