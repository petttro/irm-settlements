using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.ServiceCenters;

public record GetServiceCentersQuery(string? Search, string ServiceCompanySapId, int PageSize) : IServiceCompanyResource
{
    public static class GetServiceCentersQueryHandler
    {
        public static async Task<GetServiceCentersResult> Handle(
            GetServiceCentersQuery query,
            IPermissionsService permissionsService,
            IServiceCenterRepository serviceCenterRepository,
            CancellationToken cancellationToken)
        {
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ServiceCenterRead, query);

            var serviceCenters = await serviceCenterRepository
                .SearchAsync(query.Search, query.ServiceCompanySapId, query.PageSize, cancellationToken);

            return new GetServiceCentersResult
            {
                Data = serviceCenters.Select(GetServiceCenterResult.FromEntity).ToList()
            };
        }
    }
}
