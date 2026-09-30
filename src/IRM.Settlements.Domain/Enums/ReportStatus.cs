using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

public enum ReportStatus
{
    [Description("Черновик")]
    Draft = 0,

    [Description("Передан в оплату")]
    SentToPayment = 1,

    [Description("Оплачен")]
    Paid = 2
}
