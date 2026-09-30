using FluentValidation;
using IRM.Settlements.Api.Contracts.Reports;

namespace IRM.Settlements.Api.Contracts.Validation;

public class UpdateReportItemsRequestValidator : AbstractValidator<UpdateReportItemsRequest>
{
    public UpdateReportItemsRequestValidator()
    {
        RuleFor(x => x.Operations)
            .NotNull()
            .NotEmpty()
            .WithMessage("Operations array required");

        RuleForEach(x => x.Operations)
            .ChildRules(op =>
            {
                op.RuleFor(x => x.Type)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Operations[].Type is required");

                op.RuleFor(x => x.CouponNumber)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Operations[].CouponNumber is required");
            });
    }
}
