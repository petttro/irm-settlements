namespace IRM.Settlements.Infrastructure.PaymentGateway.Contracts;

public record GetPaymentResponse
{
    /// <summary>
    /// Id платежа
    /// </summary>
    public Guid PaymentId { get; init; }

    /// <summary>
    /// Id мерчанта
    /// </summary>
    public int MerchantId { get; init; }


    /// <summary>
    /// Дата создания платежа
    /// </summary>
    public DateTime CreateDt { get; init; }

    /// <summary>
    /// Id последнего статуса
    /// </summary>
    public int LastStatusId { get; init; }


    /// <summary>
    /// Дата установки последнего статуса
    /// </summary>
    public DateTime LastStatusDt { get; init; }

    /// <summary>
    /// Комментарий к последнему статусу
    /// </summary>
    public string? LastStatusDescription { get; init; }

    /// <summary>
    /// История статусов
    /// </summary>
    public List<GetPaymentStatusResponse> Statuses { get; init; } = [];
}

public record GetPaymentStatusResponse
{
    /// <summary>
    /// Id статуса
    /// </summary>
    public int StatusId { get; init; }

    /// <summary>
    /// Комментарий к статусу
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата установки статуса
    /// </summary>
    public DateTime Dt { get; init; }
}
