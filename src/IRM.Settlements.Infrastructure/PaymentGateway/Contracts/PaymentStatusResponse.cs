namespace IRM.Settlements.Infrastructure.PaymentGateway.Contracts;

public record PaymentStatusResponse
{
    /// <summary>
    /// id статуса
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// наименование статуса
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// признак конечного статуса, после которого нужно перестать опрашивать систему
    /// </summary>
    public bool IsFinal {get; init;}

    /// <summary>
    ///  признак, положительный ли статус
    /// </summary>
    public bool? IsPositive { get; init; }
}
