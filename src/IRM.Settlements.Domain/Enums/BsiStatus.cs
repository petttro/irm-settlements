using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

/// <summary>
/// Статус обслуживания
/// </summary>
public enum BsiStatus
{
    [Description("Новая установка")]
    New = 0,
    [Description("Установка в процессе")]
    InProgress = 1,
    [Description("Установка выполнена")]
    Complete = 3,
    [Description("Установка отменена")]
    Cancel = 4,
    [Description("Брак техники до установки")]
    MaterialDefect = 5,
    [Description("Брак техники после установки")]
    MaterialDefectAfterInstallation = 6,
    [Description("")]
    TakenFromClient = 7
}
