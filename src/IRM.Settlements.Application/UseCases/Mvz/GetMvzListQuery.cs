using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Mvz;

public record GetMvzListQuery(
    string? Search,
    Paging Paging)
{
    public static class GetMvzQueryHandler
    {
        public static async Task<GridResult<MvzItemResult>> Handle(
            GetMvzListQuery query,
            IPermissionsService permissionsService,
            IMvzRepository mvzRepository,
            CancellationToken cancellationToken)
        {
            if (!permissionsService.HasPermission(PermissionTypes.MvzReadAll))
                throw new ForbiddenException("Нет доступа к справочнику МВЗ");

            var mvz = await mvzRepository.SearchAsync(query.Search, query.Paging, cancellationToken);
            var totalCount = await mvzRepository.CountAsync(query.Search, cancellationToken);

            return new GridResult<MvzItemResult>
            {
                TotalSize =totalCount,
                PaginatedQuery = new PaginatedQuery
                {
                    PageIndex = query.Paging.Page,
                    PageSize = query.Paging.PageSize,
                },
                Data = mvz.Select(MvzItemResult.FromEntity).ToList()
            };
        }
    }
}
