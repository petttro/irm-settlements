using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IRM.Settlements.Api.Settings;

public sealed class JwtSettings
{
    public static string SectionName { get; set; } = nameof(JwtSettings);

    public required string ValidAudience { get; set; }
    public required string ValidIssuer { get; set; }
    public string RoleClaimType { get; set; } = ClaimTypes.Role;
    public string NameClaimType { get; set; } = ClaimTypes.Name;
    public string ServiceCompanyClaimType { get; set; } = "service_company_sap_id";
    public string ShopIdClaimType { get; set; } = "shop_id";
    public required string IssuerSigningKeySecret { get; set; }
    public bool IssuerSigningKeySecretIsBase64 { get; set; }
    public SymmetricSecurityKey IssuerSigningKey
    {
        get
        {
            if (IssuerSigningKeySecretIsBase64)
            {
                return new(Convert.FromBase64String(IssuerSigningKeySecret));
            }
            return new(Encoding.UTF8.GetBytes(IssuerSigningKeySecret));
        }
    }
    public required string TokenDecryptionKeySecret { get; set; }
    public bool EncryptionEnabled => !string.IsNullOrWhiteSpace(TokenDecryptionKeySecret);

    public SymmetricSecurityKey TokenDecryptionKey => new(Encoding.UTF8.GetBytes(TokenDecryptionKeySecret));
}
