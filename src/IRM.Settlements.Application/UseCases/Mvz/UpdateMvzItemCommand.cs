using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Mvz;

public record UpdateMvzItemCommand(string ShopName, string MvzCode, string MvzName)
{
    public static class UpdateMvzItemCommandHandler
    {
        public static async Task<MvzItemResult> Handle(
            UpdateMvzItemCommand command,
            IPermissionsService permissionsService,
            IMvzRepository mvzRepository,
            IUnitOfWork uow,
            CancellationToken cancellationToken)
        {
            if (!permissionsService.HasPermission(PermissionTypes.MvzWrite))
                throw new ForbiddenException("Нет доступа к справочнику МВЗ");

            var mvzItem = await mvzRepository.GetByIdAsync(command.ShopName, cancellationToken);
            if (mvzItem == null)
                throw new NotFoundException("МВЗ не найдено");

            mvzItem.MvzCode = command.MvzCode;
            mvzItem.MvzName = command.MvzName;
            mvzItem.UpdatedAt = DateTime.UtcNow;

            await uow.CommitAsync(cancellationToken);

            return MvzItemResult.FromEntity(mvzItem);
        }
    }
}
