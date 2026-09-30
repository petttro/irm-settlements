namespace IRM.Settlements.Domain.Permissions;

public class PermissionContext
{
    public string? UserServiceCompanyId { get; init; }
    public string? ReportServiceCompanyId { get; init; }

    public bool IsOwn => UserServiceCompanyId == ReportServiceCompanyId;
}
