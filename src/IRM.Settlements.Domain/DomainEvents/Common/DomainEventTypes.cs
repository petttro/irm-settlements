namespace IRM.Settlements.Domain.DomainEvents.Common;

public static class DomainEventTypes
{
    public const string AppealAddedToReport = "appeal-added-to-report";
    public const string AppealRemovedFromReport = "appeal-removed-from-report";
    public const string AppealPriceChanged = "appeal-price-changed";
    public const string ReportCreated = "report-created";
    public const string ReportDeleted = "report-deleted";
    public const string ReportSentToPayment = "report-sent-to-payment";
    public const string ReportPaid = "report-paid";
}
