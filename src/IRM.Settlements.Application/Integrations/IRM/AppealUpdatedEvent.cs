using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.Integrations.IRM;

/// <summary>
/// Команда для создания или обновления ЗНУ из Kafka.
/// </summary>
/// <param name="AppealUpdatedMessage">Данные ЗНУ из Kafka.</param>
public record AppealUpdatedEvent(AppealUpdatedMessage AppealUpdatedMessage)
{
    /// <summary>
    /// Обработчик команды создания ЗНУ.
    /// </summary>
    public static class AppealUpdatedEventHandler
    {
        public static async Task Handle(AppealUpdatedEvent @event, IAppealService appealService, CancellationToken cancellationToken)
        {
            if (!ShouldHandle(@event.AppealUpdatedMessage.Snapshot))
                return;

            if (@event.AppealUpdatedMessage.Snapshot.ServiceDate == null)
                throw new ArgumentException($"ServiceDate is missing! AppealId: {@event.AppealUpdatedMessage.Id}");

            await appealService.AddOrUpdateAsync(@event, cancellationToken);
        }

        private static bool ShouldHandle(AppealUpdatedSnapshot appeal)
        {
            // Тип проекта = Установка техники (ProjectTypeId = 2)
            var isEquipmentInstallation = appeal.ProjectTypeId is (int)ProjectType.EquipmentInstallation;

            // Платежный статус пустой (PaymentStatus = 0 или 4)
            var validPayment = appeal.PaymentStatus is (int)PaymentStatus.Inactive or (int)PaymentStatus.Empty;

            // "Установка выполнена" либо "Брак техники до установки" либо "Брак техники после установки" (BsiStatus = 3 или 5 или 6)
            var validBsiStatus = appeal.BsiStatus is
                (int)BsiStatus.Complete or
                (int)BsiStatus.MaterialDefect or
                (int)BsiStatus.MaterialDefectAfterInstallation;

            return isEquipmentInstallation && validPayment && validBsiStatus;
        }
    }
}
