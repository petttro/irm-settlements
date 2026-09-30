namespace IRM.Settlements.Domain.Entities;

public class ServiceCompany : DomainEntity<int>
{
    public required string SapId { get; set; }
    public required string Name { get; set; }
    public required DateTime UpdatedAt { get; set; }
}
