using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Domain.Entities;

public class Appeal: DomainEntity<int>
{
    public required string CouponNumber { get; set; }
    public string? SaleOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime Created { get; set; }
    public DateTime SaleDate { get; set; }
    public DateOnly ServiceDate { get; set; }
    public required string WareCode { get; set; }
    public required string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
    public int BrandId { get; set; }

    public required string CityKisId  { get; set; }
    public required string CityName  { get; set; }

    public required int ServiceCompanyId { get; set; }
    public ServiceCompany ServiceCompany { get; set; } = null!;

    public required int ServiceCenterId { get; set; }
    public ServiceCenter ServiceCenter { get; set; } = null!;

    public required string ShopName { get; set; }

    public List<AdditionalService> AdditionalServices { get; set; } = [];

    public BsiStatus BsiStatus  { get; set; }
    public PaymentStatus PaymentStatus  { get; set; }
    public ConfirmType? ConfirmType { get; set; }
    public int ProjectTypeId { get; set; }
    public Guid? ReportId { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public int CheckNumber { get; set; }
}
