using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

public enum PaymentStatus
{
    [Description("Не выставлена")]
    Inactive = 0,
    [Description("Добавлена в отчет")]
    AddedToTheReport = 1,
    [Description("Отчет на доработке")]
    ReportOnTheRevision = 2,
    [Description("Оплата")]
    Paid = 3,
    [Description("")]
    Empty = 4
}
