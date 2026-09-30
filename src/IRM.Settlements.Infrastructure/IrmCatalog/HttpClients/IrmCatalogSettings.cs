namespace IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;

public class IrmCatalogSettings
{
    public static string SectionName => nameof(IrmCatalogSettings);

    public required string BaseUrl { get; set; }

    public required string ApiKey { get; set; }

    public int TimeoutSeconds { get; set; } = 10;
}
