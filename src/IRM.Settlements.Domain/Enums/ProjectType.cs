using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

public enum ProjectType
{
    [Description("Цифровой помощник")]
    DigitalAssistant = 1,

    [Description("Установка техники")]
    EquipmentInstallation = 2,

    [Description("Цифровой помощник в магазине")]
    DigitalAssistantInStore = 4,

    [Description("Лицензионное программное обеспечение")]
    LicensedSoftware = 35,

    [Description("Утилизация бытовой техники")]
    Recycling = 39,

    [Description("Платный ремонт")]
    PaidRepair = 40,

    [Description("Тип проекта не удалось определить")]
    Unknown = -1
}