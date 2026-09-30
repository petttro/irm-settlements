namespace IRM.Settlements.Domain.Entities;

public class MvzItem : DomainEntity<string>
{
    public string? MvzCode { get; set; }

    public string? MvzName { get; set; }

    public DateTime UpdatedAt { get; set; }
}
