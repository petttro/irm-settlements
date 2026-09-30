using FluentValidation;
using IRM.Settlements.Api.Contracts.Reports;

namespace IRM.Settlements.Api.Contracts.Validation;

public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.ServiceCompanySapId)
            .NotNull()
            .NotEmpty()
            .WithMessage("ServiceCompanyId is required");

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
