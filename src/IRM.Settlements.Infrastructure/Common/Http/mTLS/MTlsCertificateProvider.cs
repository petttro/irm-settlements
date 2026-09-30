using System.Security.Cryptography.X509Certificates;
using IRM.Settlements.Infrastructure.Common.Http.mTLS.Settings;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.Common.Http.mTLS;

public interface IMTlsCertificateProvider
{
    X509Certificate2 Certificate { get; }
}

internal class MTlsCertificateProvider : IMTlsCertificateProvider
{
    public X509Certificate2 Certificate { get; }

    public MTlsCertificateProvider(IOptions<MTlsAuthSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var certBytes = Convert.FromBase64String(options.Value.ClientCertificate.Data);

        var keyStorageFlags = X509KeyStorageFlags.MachineKeySet;
        if (!OperatingSystem.IsMacOS())
            keyStorageFlags |= X509KeyStorageFlags.EphemeralKeySet;

        Certificate = X509CertificateLoader.LoadPkcs12(
            certBytes,
            options.Value.ClientCertificate.Password,
            keyStorageFlags
        );
    }
}
