using IRM.Settlements.Application.Common;

namespace IRM.Settlements.Application.QueryFilters;

public class AppealsFilter
{
    public required string ServiceCompanySapId { get; set; }
    public List<string> ServiceCenterExternalIds { get; set; } = [];
    public required DateOnlyRange ServiceDate { get; set; }
    public bool IncludeAddedToReport { get; set; }
}
