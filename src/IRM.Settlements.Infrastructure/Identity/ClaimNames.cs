using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Infrastructure.Identity;

public static class ClaimNames
{
    public static readonly string IrmRole = nameof(IrmRoles);
    public static readonly string Role = "role";
    public static readonly string ServiceCompany = "service_company_sap_id";
    public static readonly string Shop = "shop_id";
    public static readonly string UserDisplayName = "display_name";
    public static readonly string EmployeeNumber = "employee_number";
    public static readonly string UserLogin = "unique_name";
}
