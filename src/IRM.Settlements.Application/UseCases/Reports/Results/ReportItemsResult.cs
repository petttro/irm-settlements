using IRM.Settlements.Application.Common;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.UseCases.Reports.Results;

public class ReportItemsResult : GridResult<ReportItemResult>;

public class ReportItemResult
{
    public Guid Id { get; set; }
    public required string CouponNumber { get; set; }
    public string? SaleOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public DateOnly ServiceDate { get; set; }
    public required string ServiceName { get; set; }
    public required string CityKisId { get; set; }
    public required string CityName { get; set; }
    public decimal Cost { get; set; }
    public required string ServiceCenterExternalId { get; set; }
    public required string ServiceCenterName { get; set; }
    public ConfirmType? ConfirmType { get; set; }
    public List<AdditionalServiceResult> AdditionalServices { get; set; } = [];

    public static ReportItemResult FromEntity(ReportItem reportItem)
    {
        return new ReportItemResult
        {
            Id = reportItem.Id,
            CouponNumber = reportItem.CouponNumber,
            SaleOrderNumber = reportItem.SaleOrderNumber,
            OrderNumber = reportItem.OrderNumber,
            ServiceName = reportItem.ServiceName,
            ServiceDate = reportItem.ServiceDate,
            SaleDate = reportItem.SaleDate,
            ServiceCenterExternalId = reportItem.ServiceCenterExternalId,
            ServiceCenterName = reportItem.ServiceCenterName,
            CityKisId = reportItem.CityKisId,
            CityName = reportItem.CityName,
            Cost = reportItem.Cost,
            ConfirmType = reportItem.ConfirmType,
            AdditionalServices = reportItem.AdditionalServices
                .Select(s => new AdditionalServiceResult
                {
                    Name = s.Name,
                    Cost = s.Cost
                }).ToList()
        };
    }
}

public class AdditionalServiceResult
{
    public required string Name { get; set; }
    public decimal Cost { get; set; }
}
