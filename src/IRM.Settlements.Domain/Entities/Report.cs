using IRM.Settlements.Domain.DomainEvents;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Domain.Entities;

public sealed class Report : DomainEntity<Guid>
{
    private readonly List<ReportItem> _items = [];

    public IReadOnlyCollection<ReportItem> Items => _items;

    /// <summary>
    /// Имя отчета
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Порядковый номер отчета. Уникальный в рамках года
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// С даты установки
    /// </summary>
    public required DateOnly ServiceDateFrom { get; set; }

    /// <summary>
    /// По дату установки
    /// </summary>
    public required DateOnly ServiceDateTo { get; set; }

    /// <summary>
    /// Идентификатор сервисной компании
    /// </summary>
    public required string ServiceCompanySapId { get; set; }

    /// <summary>
    /// Название сервисной компании
    /// </summary>
    public required string ServiceCompanyName { get; set; }

    /// <summary>
    /// Список сервис центров по которым построен отчет
    /// </summary>
    public List<string> ServiceCenterExternalIds { get; set; } = [];

    /// <summary>
    /// Дата создания
    /// </summary>
    public required DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }

    /// <summary>
    /// Дата обновления
    /// </summary>
    public required DateTime UpdatedAt { get; set; }
    public required string UpdatedBy { get; set; }

    /// <summary>
    /// Дата отправки на оплату
    /// </summary>
    public DateTime? SentToPaymentDate { get; set; }

    /// <summary>
    /// Дата оплаты
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Статус отчета
    /// </summary>
    public ReportStatus Status { get; private set; } = ReportStatus.Draft;

    /// <summary>
    /// Сумма отчета
    /// </summary>
    public decimal TotalCost { get; private set; }

    /// <summary>
    /// Кол-во позиций
    /// </summary>
    public int ItemsCount { get; private set; }

    public Guid? PaymentId { get; private set; }

    private void RecalculateTotals()
    {
        ItemsCount = _items.Count;
        TotalCost = _items.Sum(i => i.TotalCost);
    }

    public void ApplyPrices(IReadOnlyCollection<PriceInfo> prices)
    {
        ArgumentNullException.ThrowIfNull(prices);
        var priceLookup = prices.ToDictionary(p => (p.WareCode, p.ShopName, p.ServiceDate));

        foreach (var item in _items)
        {
            item.ApplyPrices(priceLookup);
        }

        RecalculateTotals();
    }

    public void AddItems(List<ReportItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
            throw new ReportIncorrectStateException("Отчёт не может быть пустым. Должен содержать хотя бы один элемент.");

        _items.AddRange(items);
        RecalculateTotals();
        foreach (var item in items)
        {
            AddDomainEvent(new AppealAddedToReportEvent(item.CouponNumber, Id));
        }
    }

    public void RemoveItems(HashSet<string> couponNumbers)
    {
        ArgumentNullException.ThrowIfNull(couponNumbers);
        _items.RemoveAll(item => couponNumbers.Contains(item.CouponNumber));
        if (_items.Count == 0)
            throw new ReportIncorrectStateException("Отчёт не может быть пустым. Должен содержать хотя бы один элемент.");

        RecalculateTotals();
        foreach (var couponNumber in couponNumbers)
        {
            AddDomainEvent(new AppealRemovedFromReportEvent(couponNumber, Id));
        }
    }

    public void MarkAsSendToPayment(Guid? paymentId = null)
    {
        if (Status == ReportStatus.SentToPayment)
            return;

        ValidateNullCostItems();

        PaymentId = paymentId;
        SentToPaymentDate = DateTime.UtcNow;
        Status = ReportStatus.SentToPayment;

        AddDomainEvent(new ReportSentToPaymentEvent(Id, paymentId, TotalCost));
    }

    public void MarkAsPaid()
    {
        if (Status == ReportStatus.Paid)
            return;

        Status = ReportStatus.Paid;
        PaymentDate = DateTime.UtcNow;

        if (PaymentId.HasValue)
            AddDomainEvent(new ReportPaidEvent(Id, PaymentId.Value, TotalCost));
    }

    public void Create()
    {
        AddDomainEvent(new ReportCreatedEvent(Id));
    }

    public void Delete()
    {
        var couponNumbers = Items.Select(item => item.CouponNumber).ToHashSet();
        foreach (var couponNumber in couponNumbers)
        {
            AddDomainEvent(new AppealRemovedFromReportEvent(couponNumber, Id));
        }
        _items.Clear();

        AddDomainEvent(new ReportDeletedEvent(Id));
    }

    private void ValidateNullCostItems()
    {
        if (_items.Count == 0)
            throw new MissingSettingException("ReportItems not loaded from DB");

        var invalidCostItems = new List<string>();
        foreach (var reportItem in Items)
        {
            var isValid = reportItem.CheckNullCost();
            if (!isValid)
                invalidCostItems.Add(reportItem.CouponNumber);
        }

        if (invalidCostItems.Count > 0)
        {
            var invalidCouponNumbers = string.Join(", ", invalidCostItems);
            var errorMessage = "Отчет не может быть отправлен на оплату. Найдены услуги с нулевой ценой: " +
                               $"{invalidCouponNumbers} . " +
                               "Вы можете: удалить услуги из отчета и отправить на оплату или обратиться в центральный офис. " +
                               "После внесения цен на услуги со стороны офиса, отчет может быть передан в оплату.";

            throw new ReportIncorrectStateException(errorMessage);
        }
    }
}
