using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class TestDataRoot
{
    public required List<Appeal> Appeals { get; set; } = [];
    public required List<ServiceCompany> ServiceCompanies { get; set; } = [];
    public required List<ServiceCenter> ServiceCenters { get; set; } = [];
    public required List<MvzItem> Mvz { get; set; } = [];
}
