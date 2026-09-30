using FluentValidation;
using IRM.Settlements.Api.Contracts.Reports;

namespace IRM.Settlements.Api.Contracts.Validation;

public class ExportPaymentOrderRequestValidator : AbstractValidator<ExportPaymentOrderRequest>
{
    public ExportPaymentOrderRequestValidator()
    {
        RuleFor(x => x.ReportIds)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one ReportId is required");
    }
}