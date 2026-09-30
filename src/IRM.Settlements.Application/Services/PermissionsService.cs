using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.Services;

public class PermissionsService : IPermissionsService
{
    private readonly IUserContext _userContext;

    private static readonly Dictionary<string, PermissionKind> Map = new()
    {
        [PermissionTypes.ReportReadAll] = PermissionKind.Global,
        [PermissionTypes.ReportCreate] = PermissionKind.Global,
        [PermissionTypes.ReportExportRegistry] = PermissionKind.Global,
        [PermissionTypes.ServiceCompanyReadAll] = PermissionKind.Global,
        [PermissionTypes.MvzWrite] = PermissionKind.Global,
        [PermissionTypes.MvzReadAll] = PermissionKind.Global,

        [PermissionTypes.ReportRead] = PermissionKind.Scoped,
        [PermissionTypes.ReportWrite] = PermissionKind.Scoped,
        [PermissionTypes.ReportDelete] = PermissionKind.Scoped,
        [PermissionTypes.ReportRecalculate] = PermissionKind.Scoped,
        [PermissionTypes.ReportSendToPayment] = PermissionKind.Scoped,
        [PermissionTypes.ReportConfirmPayment] = PermissionKind.Scoped,
        [PermissionTypes.ReportExport] = PermissionKind.Scoped,
        [PermissionTypes.ServiceCenterRead] = PermissionKind.Scoped,
        [PermissionTypes.PaymentOrderExport] = PermissionKind.Scoped
    };

    public PermissionsService(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public void CheckPermissionOrThrow(string permission, IServiceCompanyResource resource)
    {
        var permissions = GetPermissionsInternal(resource.ServiceCompanySapId, null);
        if (!permissions.Contains(permission))
            throw new ForbiddenException($"Action {permission} not allowed for ServiceCompany {resource.ServiceCompanySapId}");
    }

    public void CheckPermissionOrThrow(string permission, Report report)
    {
        var permissions = GetPermissionsInternal(report.ServiceCompanySapId, report.Status);
        if (!permissions.Contains(permission))
            throw new ForbiddenException($"Action {permission} not allowed for {report.ServiceCompanySapId} in Status {report.Status}");
    }

    public bool HasPermission(string permission)
    {
        var rules = PermissionMatrix.Rules
            .Where(r => _userContext.GetRoles().Contains(r.Role));

        return rules.Any(r => r.Permissions.Contains(permission));
    }

    public HashSet<string> GetGlobalPermissions()
    {
        return PermissionMatrix.Rules
            .Where(r => _userContext.GetRoles().Contains(r.Role))
            .SelectMany(r => r.Permissions)
            .Where(p => Map[p] == PermissionKind.Global)
            .ToHashSet();
    }

    public HashSet<string> GetReportPermissions(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return GetPermissionsInternal(report.ServiceCompanySapId, report.Status, PermissionKind.Scoped);
    }

    private HashSet<string> GetPermissionsInternal(
        string? serviceCompanySapId, ReportStatus? reportStatus, PermissionKind? permissionKind = null)
    {
        var context = new PermissionContext
        {
            UserServiceCompanyId = _userContext.ServiceCompanySapId,
            ReportServiceCompanyId = serviceCompanySapId
        };

        var rules = PermissionMatrix.Rules
            .Where(r => _userContext.GetRoles().Contains(r.Role))
            .Where(r => r.Condition(context));

        if (reportStatus.HasValue)
            rules = rules.Where(r => r.ReportStatus == reportStatus);

        var permissions = rules.SelectMany(r => r.Permissions);

        if (permissionKind.HasValue)
            permissions = permissions.Where(p => Map[p] == PermissionKind.Scoped);

        return permissions.ToHashSet();
    }
}
