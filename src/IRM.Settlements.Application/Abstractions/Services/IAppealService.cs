using IRM.Settlements.Application.Integrations.IRM;

namespace IRM.Settlements.Application.Abstractions.Services;

public interface IAppealService
{
    Task AddOrUpdateAsync(AppealUpdatedEvent updatedEvent, CancellationToken cancellationToken = default);
}
