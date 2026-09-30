namespace IRM.Settlements.Domain.Entities;

public class ServiceCenter : DomainEntity<int>
{
    public required string ExternalId { get; set; }
    public required string Name { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string ServiceCompanySapId { get; set; }
}
