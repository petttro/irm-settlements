using System.ComponentModel;

namespace IRM.Settlements.Domain.Enums;

[Flags]
public enum IrmRoles
{
    [Description("Администратор")] Administrator = 1 << 0,
    [Description("Горячая Линия")] HotLine = 1 << 1,
    [Description("Call-центр")] CallCenter = 1 << 2,
    [Description("Сервисная компания")] ServiceCompany = 1 << 3,
    [Description("Магазин")] Store = 1 << 4,
    [Description("Центральный офис")] CentralOffice = 1 << 5,
    [Description("Старший менеджер")] SeniorManager = 1 << 6,
    [Description("Тех. поддержка")] Support = 1 << 7,
    [Description("Клиент API CRM")] ApiClient_CRM = 1 << 8,
    [Description("Call-центр с резервом квот")] CallCenterWithQuotaReserve = 1 << 9,
    [Description("Клиент API IRM-Master")] ApiClient_IRM_Master = 1 << 10,
    [Description("Клиент API Quotas")] ApiClient_Quotas = 1 << 11,
    [Description("Сервисная компания с резервом квот")] ServiceCompanyWithQuotaReserve = 1 << 12,
    [Description("Группа разбора")] InvestigationGroup = 1 << 13,
    [Description("Клиент API IRM-Incidents")] ApiClient_IRM_Incidents = 1 << 14,
    [Description("Клиент API IRM-InstallationQueue")] ApiClient_IRM_InstallationQueue = 1 << 15,
    [Description("Утилизатор")] Recycling_Recycler = 1 << 16,
    [Description("Магазин утилизации")] Recycling_Store = 1 << 17,
    [Description("Менеджер утилизации")] Recycling_Manager = 1 << 18,
    [Description("Call-центр утилизации")] Recycling_CallCenter = 1 << 19,
    [Description("Интеграция с микросервисами")] Integration = 1 << 20
}
