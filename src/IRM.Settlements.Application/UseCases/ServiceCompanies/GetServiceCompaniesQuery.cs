using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.ServiceCompanies;

public record GetServiceCompaniesQuery(string? Search, int PageSize)
{
    public static class GetServiceCompaniesQueryHandler
    {
        public static async Task<GetServiceCompaniesResult> Handle(
            GetServiceCompaniesQuery query,
            IUserContext userContext,
            IPermissionsService permissionsService,
            IServiceCompanyRepository serviceCompanyRepository,
            CancellationToken cancellationToken)
        {
            List<ServiceCompany> serviceCompanies = [];
            if (permissionsService.HasPermission(PermissionTypes.ServiceCompanyReadAll))
            {
                serviceCompanies = await serviceCompanyRepository.SearchAsync(query.Search, query.PageSize, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(userContext.ServiceCompanySapId))
            {
                var serviceCompany = await serviceCompanyRepository.GetBySapIdAsync(userContext.ServiceCompanySapId, cancellationToken);
                serviceCompanies = serviceCompany != null ? [serviceCompany] : [];
            }

            return new GetServiceCompaniesResult
            {
                Data = serviceCompanies.Select(GetServiceCompanyResult.FromEntity).ToList()
            };
        }
    }
}
