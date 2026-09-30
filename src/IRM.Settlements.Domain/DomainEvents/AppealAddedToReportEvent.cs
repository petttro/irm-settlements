using IRM.Settlements.Domain.DomainEvents.Common;

namespace IRM.Settlements.Domain.DomainEvents;

public record AppealAddedToReportEvent(string CouponNumber, Guid ReportId) : DomainEvent
{
    public override string EventType => DomainEventTypes.AppealAddedToReport;
}
