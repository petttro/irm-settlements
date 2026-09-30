using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.UseCases.ServiceCenters;

public class GetServiceCentersResult
{
    public List<GetServiceCenterResult> Data { get; set; } = [];
}

public class GetServiceCenterResult
{
    public required string ExternalId { get; set; }
    public required string Name { get; set; }

    public static GetServiceCenterResult FromEntity(ServiceCenter serviceCenter)
    {
        return new GetServiceCenterResult
        {
            Name = serviceCenter.Name,
            ExternalId = serviceCenter.ExternalId
        };
    }
}
