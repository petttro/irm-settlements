using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IRM.Settlements.Infrastructure.Common.Http.mTLS.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.Common.Http.mTLS;

internal class MTlsHttpHandlerFactory
{
    private readonly IMTlsCertificateProvider _mTlsCertificateProvider;
    private readonly ILogger<MTlsHttpHandlerFactory> _logger;
    private readonly MTlsAuthSettings _settings;

    public MTlsHttpHandlerFactory(
        IMTlsCertificateProvider mTlsCertificateProvider,
        ILogger<MTlsHttpHandlerFactory> logger,
        IOptions<MTlsAuthSettings> options)
    {
        _mTlsCertificateProvider = mTlsCertificateProvider;
        _logger = logger;
        _settings = options.Value;
    }

    public HttpMessageHandler Create()
    {
        return new SocketsHttpHandler
        {
            MaxConnectionsPerServer = _settings.MaxConnectionsPerServer,
            PooledConnectionLifetime = _settings.PooledConnectionLifetime,
            PooledConnectionIdleTimeout = _settings.PooledConnectionIdleTimeout,

            // TLS config
            SslOptions = new SslClientAuthenticationOptions
            {
                ClientCertificates = new X509CertificateCollection { _mTlsCertificateProvider.Certificate },
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                                      | System.Security.Authentication.SslProtocols.Tls13,
                RemoteCertificateValidationCallback = (_, certificate, chain, errors) =>
                {
                    // настройка для отключения проверки сервисного сертификата
                    if (!_settings.ValidateServerCertificate)
                        return true;

                    if (errors == SslPolicyErrors.None)
                        return true;

                    LogCertificateError(certificate, chain, errors);
                    return false;
                }
            }
        };
    }

    private void LogCertificateError(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        var cert2 = certificate as X509Certificate2;
        var chainErrors = new List<string>();

        if (chain?.ChainStatus != null)
        {
            foreach (var status in chain.ChainStatus)
            {
                chainErrors.Add($"{status.Status}: {status.StatusInformation.Trim()}");
            }
        }

        _logger.LogError(
            "mTLS validation failed. " +
            "Errors: {Errors}, Subject: {Subject}, Issuer: {Issuer}, Thumbprint: {Thumbprint}, NotAfter: {NotAfter}, " +
            "ChainErrors: {ChainErrors}",
            errors, cert2?.Subject, cert2?.Issuer, cert2?.Thumbprint, cert2?.NotAfter,
            string.Join(" | ", chainErrors)
        );
    }
}
