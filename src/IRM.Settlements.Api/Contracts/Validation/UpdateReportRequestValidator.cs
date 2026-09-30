using FluentValidation;
using IRM.Settlements.Api.Contracts.Reports;

namespace IRM.Settlements.Api.Contracts.Validation;

public class UpdateReportRequestValidator : AbstractValidator<UpdateReportRequest>
{
    public UpdateReportRequestValidator()
    {
        RuleFor(x => x.ServiceDateFrom)
            .NotNull()
            .NotEmpty()
            .WithMessage("ServiceDateTo is required");

        RuleFor(x => x.ServiceDateTo)
            .NotNull()
            .NotEmpty()
            .WithMessage("ServiceDateTo is required");

        RuleFor(x => x.ServiceDateTo)
            .GreaterThanOrEqualTo(x => x.ServiceDateFrom)
            .WithMessage("ServiceDateFrom should greater or equal ServiceDateTo");
    }
}
