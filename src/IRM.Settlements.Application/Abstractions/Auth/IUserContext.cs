using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.Abstractions.Auth;

public interface IUserContext
{
    string UserName { get; }

    string? ServiceCompanySapId { get; }

    bool IsInRole(IrmRoles role);

    IReadOnlyCollection<IrmRoles> GetRoles();

    bool IsAuthenticated { get; }
}
