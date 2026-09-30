using IRM.Settlements.Application.Abstractions.Services;

namespace IRM.Settlements.Application.UseCases.Permissions;

public record GetPermissionsQuery
{
    public static class GetPermissionsQueryHandler
    {
        public static async Task<IEnumerable<string>> Handle(GetPermissionsQuery query, IPermissionsService permissionsService)
        {
            return permissionsService.GetGlobalPermissions();
        }
    }
}
