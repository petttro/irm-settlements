using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Application.Integrations.IRM;

public class AppealUpdatedMessage
{
    public int Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public string? UserLogin { get; set; }
    public required AppealUpdatedSnapshot Snapshot { get; set; }

    public static Appeal ToEntity(AppealUpdatedMessage message)
    {
        return new Appeal
        {
            Id = message.Snapshot.Id,
            CouponNumber = message.Snapshot.CouponNumber,
            OrderNumber = message.Snapshot.OrderNumber,
            SaleOrderNumber = message.Snapshot.SaleOrderNumber,
            BrandId = message.Snapshot.BrandId,

            Created = message.Snapshot.Created,
            SaleDate = message.Snapshot.SaleDate,
            ServiceDate = DateOnly.FromDateTime(message.Snapshot.ServiceDate!.Value),

            ServiceName = message.Snapshot.ServiceName,
            ServicePrice = message.Snapshot.ServicePrice,

            ServiceCompanyId = message.Snapshot.ServiceCompanyId,
            ServiceCenterId = message.Snapshot.ResolverId,

            CityKisId = message.Snapshot.CityKisId,
            CityName = message.Snapshot.CityName,

            ShopName = message.Snapshot.ShopName,
            WareCode = message.Snapshot.WareCode,
            BsiStatus = (BsiStatus)message.Snapshot.BsiStatus,
            PaymentStatus = (PaymentStatus)message.Snapshot.PaymentStatus,
            ConfirmType = message.Snapshot.ConfirmType.HasValue ? (ConfirmType)message.Snapshot.ConfirmType.Value : null,
            ProjectTypeId = message.Snapshot.ProjectTypeId,
            CheckNumber = message.Snapshot.CheckNumber,

            AdditionalServices = message.Snapshot.AdditionalServices
                .Select(s => new AdditionalService
                {
                    Name = s.Name,
                    WareCode = s.SapId
                })
                .ToList(),

            UpdatedAt = message.CreatedAt
        };
    }
}

public class AppealUpdatedSnapshot
{
    public int Id { get; set; }
    public required string CouponNumber { get; set; }
    public string? SaleOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime Created { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime? ServiceDate { get; set; }
    public DateTime? AppealDate { get; set; }
    public required string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
    public int BrandId { get; set; }

    public required string CityKisId { get; set; }
    public required string CityName { get; set; }

    public int ServiceCompanyId { get; set; }

    // ServiceCompanySapId
    public required string EnterpriseId { get; set; }
    public required string ServiceCompanyName { get; set; }

    // ServiceCenterId
    public int ResolverId { get; set; }
    public required string ResolverExternalId { get; set; }
    public required string ResolverNameResolver { get; set; }

    public required string WareCode { get; set; }
    public int BsiStatus { get; set; }
    public int PaymentStatus { get; set; }
    public int ProjectTypeId { get; set; }
    public int ShopId { get; set; }
    public required string ShopName { get; set; }
    public int? ConfirmType { get; set; }
    public int CheckNumber { get; set; }

    public List<AdditionalServiceMessage> AdditionalServices { get; set; } = [];
}

public class AdditionalServiceMessage
{
    public required string Name { get; set; }
    public required string SapId { get; set; }

    public decimal? Price { get; set; }
    public int? ZnuServices { get; set; }
    public bool? IrmServicePrice { get; set; }
    public bool? IsDeleted { get; set; }
}
