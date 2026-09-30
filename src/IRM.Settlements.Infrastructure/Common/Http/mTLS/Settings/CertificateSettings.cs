namespace IRM.Settlements.Infrastructure.Common.Http.mTLS.Settings;

public class CertificateSettings
{
    /// <summary>
    /// Клиентский сертификат для mTLS аутентификации. PFX в кодировке base64
    /// </summary>
    public required string Data { get; set; }

    /// <summary>
    /// Пароль от сертификата
    /// </summary>
    public required string Password { get; set; }
}
