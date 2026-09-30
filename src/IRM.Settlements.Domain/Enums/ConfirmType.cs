using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

/// <summary>
/// Тип подтверждения
/// </summary>
public enum ConfirmType
{
    [Description("Уведомлением")]
    Push = 0,
    [Description("QR-кодом")]
    QrCode = 1,
    [Description("Документом")]
    Document = 2,
    [Description("Уведомлением или SMS")]
    PushOrSms = 3,
}
