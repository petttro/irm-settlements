using System.Security.Claims;
using System.Text.Encodings.Web;
using IRM.Settlements.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) {}

    // Пример JWT токена
    // {
    //     "unique_name": "CORP\\petnaumenko",
    //     "service_company_sap_id": [
    //     "K000007490",
    //     "K000007490"
    //         ],
    //     "display_name": "Науменко Петр",
    //     "employee_number": "00179523",
    //     "irm_role": "Сервисная компания",
    //     "nbf": 1778159484,
    //     "exp": 1778195484,
    //     "iat": 1778159484,
    //     "iss": "irm-3.0",
    //     "aud": "irm"
    // }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>();

        var userId = Request.Headers["x-test-user-id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userId))
            claims.Add(new Claim(ClaimTypes.Name, userId));

        var serviceCompanyId = Request.Headers["x-test-service-company-id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(serviceCompanyId))
            claims.Add(new Claim(ClaimNames.ServiceCompany, serviceCompanyId));

        var roles = Request.Headers["x-test-roles"].FirstOrDefault();
        if (!string.IsNullOrEmpty(roles))
        {
            foreach (var role in roles.Split(','))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        if (claims.Count == 0)
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
