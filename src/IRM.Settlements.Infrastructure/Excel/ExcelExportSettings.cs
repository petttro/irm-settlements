namespace IRM.Settlements.Infrastructure.Excel;

public class ExcelExportSettings
{
    public static string SectionName { get; set; } = nameof(ExcelExportSettings);

    public required string ProtectionPassword { get; set; }

}
