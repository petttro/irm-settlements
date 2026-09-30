using FluentValidation;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Api.Contracts.Validation;

public class ExportReportsRequestValidator : AbstractValidator<ExportReportsRequest>
{
    public ExportReportsRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage($"Incorrect ReportStatus. Valid values: {GetValidReportStatuses()}");

        RuleFor(x => x.CreatedDateFrom)
            .LessThanOrEqualTo(x => x.CreatedDateTo)
            .WithMessage("CreatedDateFrom must be <= CreatedDateTo");

        RuleFor(x => x.PaymentDateFrom)
            .LessThanOrEqualTo(x => x.PaymentDateTo)
            .WithMessage("PaymentDateFrom must be <= PaymentDateTo");

        RuleFor(x => x.SentToPaymentDateFrom)
            .LessThanOrEqualTo(x => x.SentToPaymentDateTo)
            .WithMessage("SentToPaymentDateFrom must be <= SentToPaymentDateFrom");
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        return Enum.TryParse<ReportStatus>(status, true, out var parsed)
               && Enum.IsDefined(typeof(ReportStatus), parsed);
    }

    private static string GetValidReportStatuses()
    {
        return string.Join(", ", Enum.GetNames<ReportStatus>());
    }
}
