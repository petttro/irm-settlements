using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Domain.Permissions;

public record PermissionMatrixRule(
    IrmRoles Role,
    ReportStatus ReportStatus,
    Func<PermissionContext, bool> Condition,
    string[] Permissions);
