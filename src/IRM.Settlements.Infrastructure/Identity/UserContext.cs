using System.Security.Claims;
using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace IRM.Settlements.Infrastructure.Identity;

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _context;

    public HttpUserContext(IHttpContextAccessor context)
    {
        _context = context;
    }

    public string UserName => _context.HttpContext?.User.Identity?.Name ?? "anonymous";

    public string? ServiceCompanySapId => _context.HttpContext?.User.FindFirst(ClaimNames.ServiceCompany)?.Value;

    public bool IsInRole(IrmRoles role) => _context.HttpContext?.User.IsInRole(role.ToString()) ?? false;

    public IReadOnlyCollection<IrmRoles> GetRoles()
    {
        return _context.HttpContext?.User
                   .FindAll(ClaimTypes.Role)
                   .Select(c => Enum.Parse<IrmRoles>(c.Value))
                   .ToArray()
               ?? [];
    }

    public bool IsAuthenticated => _context.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
