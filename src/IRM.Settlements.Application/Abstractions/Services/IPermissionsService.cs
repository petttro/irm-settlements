using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Abstractions.Services;

public interface IPermissionsService
{
    void CheckPermissionOrThrow(string permission, IServiceCompanyResource resource);

    void CheckPermissionOrThrow(string permission, Report report);

    bool HasPermission(string permission);

    HashSet<string> GetGlobalPermissions();

    HashSet<string> GetReportPermissions(Report report);
}
