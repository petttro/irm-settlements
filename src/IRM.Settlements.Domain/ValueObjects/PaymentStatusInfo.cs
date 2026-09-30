namespace IRM.Settlements.Domain.ValueObjects;

public record PaymentStatusInfo
{
    public Guid PaymentId { get; init; }

    public int StatusId { get; init; }

    public string? StatusName { get; init; }

    /// <summary>
    /// признак конечного статуса, после которого нужно перестать опрашивать систему
    /// </summary>
    public bool IsFinal {get; init;}

    /// <summary>
    ///  признак, положительный ли статус
    /// </summary>
    public bool? IsPositive { get; init; }
}
