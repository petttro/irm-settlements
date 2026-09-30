namespace IRM.Settlements.Infrastructure.PaymentGateway.Contracts;

public record SendPaymentRequest
{
    /// <summary>
    /// уникальный id платежа, обязательный параметр. после неуспешного запроса в API необходимо переиспользовать
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// ID мерчанта, обязательный параметр
    /// </summary>
    public required int MerchantId { get; init; }

    /// <summary>
    /// тип платежа (СМ. справочник ниже (справочник 4.3)), обязательный параметр
    /// 2 - "Товары", 3 - "Услуги", 4 - "Возвраты", 5 - "Платежи в бюджет"
    /// </summary>
    public required int PaymentTypeId { get; init; }

    /// <summary>
    /// сумма документа, обязательный параметр
    /// </summary>
    public required decimal DocAmount { get; init; }

    /// <summary>
    /// код валюты, обязательный параметр
    /// </summary>
    public required string CurrencyCode { get; init; }

    /// <summary>
    /// ИНН компании, обязательный параметр
    /// </summary>
    public required string PayerInn { get; init; }

    /// <summary>
    /// кпп компании от которой платим
    /// </summary>
    public string? PayerKpp { get; init; }

    /// <summary>
    /// наименование банка получателя, обязательный параметр (кроме Армении)
    /// </summary>
    public required string RecipientBankName { get; init; }

    /// <summary>
    /// БИК банка получателя, обязательный параметр для рф банков
    /// </summary>
    public string? RecipientBankBic { get; init; }

    /// <summary>
    /// свифт-код банка получателя (обязательно для иностранных банков)
    /// </summary>
    public string? RecipientBankSwiftCode { get; init; }

    /// <summary>
    /// корсчет банка получателя (обязательно для российских банков. для других передавать нельзя)
    /// </summary>
    public string? RecipientCorrespondentAccount { get; init; }

    /// <summary>
    /// наименование получателя, обязательный параметр
    /// </summary>
    public required string RecipientName { get; init; }

    /// <summary>
    /// счет получателя, обязательный параметр
    /// </summary>
    public required string RecipientAccount { get; init; }

    /// <summary>
    /// ИНН получателя, обязательный параметр (кроме Армении)
    /// </summary>
    public required string? RecipientInn { get; init; }

    /// <summary>
    /// КПП получателя
    /// </summary>
    public string? RecipientKpp { get; init; }

    /// <summary>
    /// назначение платежа, обязательный параметр
    /// </summary>
    public required string PaymentDescription { get; init; }
}
