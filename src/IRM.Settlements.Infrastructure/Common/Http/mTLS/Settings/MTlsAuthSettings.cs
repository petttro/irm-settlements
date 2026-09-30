namespace IRM.Settlements.Infrastructure.Common.Http.mTLS.Settings;

public class MTlsAuthSettings
{
    public static readonly string SectionName = "MTlsAuthSettings";

    public required CertificateSettings ClientCertificate { get; set; }

    public int MaxConnectionsPerServer { get; set; } = 10;

    public TimeSpan PooledConnectionLifetime { get; set; }  = TimeSpan.FromMinutes(10);

    public TimeSpan PooledConnectionIdleTimeout { get; set; }  = TimeSpan.FromMinutes(2);

    public bool ValidateServerCertificate { get; set; } = true;
}
