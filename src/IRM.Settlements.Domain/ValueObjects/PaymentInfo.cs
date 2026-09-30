namespace IRM.Settlements.Domain.ValueObjects;

public record PaymentInfo
{
    /// <summary>
    /// уникальный id платежа, обязательный параметр
    /// </summary>
    public required Guid PaymentId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Сумма к оплате
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// наименование банка получателя, обязательный параметр (кроме Армении)
    /// </summary>
    public required string RecipientBankName { get; set; }

    /// <summary>
    /// БИК банка получателя, обязательный параметр для рф банков
    /// </summary>
    public string? RecipientBankBic { get; set; }

    /// <summary>
    /// свифт-код банка получателя (обязательно для иностранных банков)
    /// </summary>
    public string? RecipientBankSwiftCode { get; set; }

    /// <summary>
    /// корсчет банка получателя (обязательно для российских банков. для других передавать нельзя)
    /// </summary>
    public string? RecipientCorrespondentAccount { get; set; }

    /// <summary>
    /// наименование получателя, обязательный параметр
    /// </summary>
    public required string RecipientName { get; set; }

    /// <summary>
    /// счет получателя, обязательный параметр
    /// </summary>
    public required string RecipientAccount { get; set; }

    /// <summary>
    /// ИНН получателя, обязательный параметр (кроме Армении)
    /// </summary>
    public required string? RecipientInn { get; set; }

    /// <summary>
    /// КПП получателя
    /// </summary>
    public string? RecipientKpp { get; set; }
}
