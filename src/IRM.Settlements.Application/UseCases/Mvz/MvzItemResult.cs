using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.UseCases.Mvz;

public class MvzItemResult
{
    public required string ShopName { get; set; }
    public string? MvzCode { get; set; }
    public string? MvzName { get; set; }

    public static MvzItemResult FromEntity(MvzItem mvzItem)
    {
        return new MvzItemResult
        {
            ShopName = mvzItem.Id,
            MvzCode = mvzItem.MvzCode,
            MvzName = mvzItem.MvzName
        };
    }
}
