using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.UseCases.ServiceCompanies;

public class GetServiceCompaniesResult
{
    public List<GetServiceCompanyResult> Data { get; set; } = [];
}

public class GetServiceCompanyResult
{
    public required string SapId { get; set; }
    public required string Name { get; set; }

    public static GetServiceCompanyResult FromEntity(ServiceCompany entity)
    {
        return new GetServiceCompanyResult
        {
            SapId = entity.SapId,
            Name = entity.Name
        };
    }
}
