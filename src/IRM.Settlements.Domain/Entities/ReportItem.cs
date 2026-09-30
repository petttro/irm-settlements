using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Domain.Entities;

public class ReportItem : DomainEntity<Guid>
{
    public required string CouponNumber  { get; set; }
    public string? SaleOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public DateOnly ServiceDate { get; set; }
    public DateTime SaleDate { get; set; }
    public required string ServiceName { get; set; }
    public required string ServiceCompanySapId { get; set; }
    public required string ServiceCenterExternalId { get; set; }
    public required string ServiceCenterName { get; set; }
    public required string CityKisId { get; set; }
    public required string CityName { get; set; }
    public required string ShopName { get; set; }
    public required string WareCode { get; set; }
    public decimal Cost { get; private set; }
    public decimal TotalCost { get; private set; }
    public Guid ReportId { get; set; }
    public Report? Report { get; set; }
    public MvzItem? Mvz { get; set; }
    public required string SearchText {get; set;}
    public required DateTime UpdatedAt { get; set; }
    public decimal? SapPrice { get; private set; }
    public string? SapNds { get; private set; }
    public int CheckNumber { get; set; }
    public ConfirmType? ConfirmType { get; set; }
    public BsiStatus BsiStatus { get; set; }
    public List<AdditionalService> AdditionalServices { get; set; } = [];

    private void SetPrice(decimal price, string nds)
    {
        // Если цена поменялась в SAP
        if (SapPrice.HasValue && SapPrice != price)
            AddDomainEvent(new AppealPriceChangedEvent(CouponNumber, SapPrice.Value, price));

        SapPrice = price;
        SapNds = nds;
    }

    // BsiStatus.Complete => только цена основной услуги
    // BsiStatus.MaterialDefect => только сумма цен доп. услуг
    // BsiStatus.MaterialDefectAfterInstallation => цена основной услуги + сумма доп. услуг
    public void ApplyPrices(IReadOnlyDictionary<(string WareCode, string Shop, DateOnly Date), PriceInfo> priceLookup)
    {
        var applyMainCost = BsiStatus is BsiStatus.Complete or BsiStatus.MaterialDefectAfterInstallation;
        var applyAdditionalCost = BsiStatus is BsiStatus.MaterialDefect or BsiStatus.MaterialDefectAfterInstallation;

        if(priceLookup.TryGetValue((WareCode, ShopName, ServiceDate), out var mainPrice))
            SetPrice(mainPrice.Price, mainPrice.Nds);

        // Основная услуга
        if (applyMainCost)
            Cost = SapPrice ?? 0;
        else
            Cost = 0;

        // Доп услуги
        foreach (var additionalService in AdditionalServices)
        {
            if (priceLookup.TryGetValue((additionalService.WareCode, ShopName, ServiceDate), out var servicePrice))
                additionalService.SetPrice(servicePrice.Price, servicePrice.Nds);

            if (applyAdditionalCost)
                additionalService.Cost = additionalService.SapPrice ?? 0;
            else
                additionalService.Cost = 0;
        }

        TotalCost = Cost + AdditionalServices.Sum(s => s.Cost);
    }

    public bool CheckNullCost()
    {
        // Если статус обслуживания = "Установка произведена" (BsiStatus = 3), то основная услуга не должна быть равна нулю.
        // Если статус обслуживания = "Брак техники до установки" (BsiStatus = 5) либо "Брак техники после установки" (BsiStatus = 6),
        // то сумма услуг не должна быть равна нулю.

        if (BsiStatus is BsiStatus.Complete)
            return Cost > 0;

        if (BsiStatus is BsiStatus.MaterialDefect or BsiStatus.MaterialDefectAfterInstallation)
            return AdditionalServices.Exists(a => a.Cost > 0);

        return false;
    }

    public IEnumerable<string> GetWareCodes()
    {
        yield return WareCode;

        foreach (var service in AdditionalServices)
            yield return service.WareCode;
    }

    public static ReportItem FromAppeal(Appeal appeal)
    {
        return new ReportItem
        {
            CouponNumber = appeal.CouponNumber,
            SaleOrderNumber = appeal.SaleOrderNumber,
            OrderNumber = appeal.OrderNumber,
            SearchText = $"{appeal.CouponNumber}-{appeal.SaleOrderNumber}-{appeal.OrderNumber}",
            ServiceDate = appeal.ServiceDate,
            SaleDate = appeal.SaleDate,
            ServiceName = appeal.ServiceName,
            ServiceCenterExternalId = appeal.ServiceCenter.ExternalId,
            ServiceCenterName = appeal.ServiceCenter.Name,
            CityKisId = appeal.CityKisId,
            CityName = appeal.CityName,
            ShopName = appeal.ShopName,
            WareCode = appeal.WareCode,
            UpdatedAt = appeal.UpdatedAt,
            ServiceCompanySapId = appeal.ServiceCompany.SapId,
            CheckNumber = appeal.CheckNumber,
            ConfirmType = appeal.ConfirmType,
            BsiStatus = appeal.BsiStatus,
            AdditionalServices = appeal.AdditionalServices
        };
    }
}
