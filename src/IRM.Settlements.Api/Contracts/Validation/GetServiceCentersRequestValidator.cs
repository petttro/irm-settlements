using FluentValidation;
using IRM.Settlements.Api.Contracts.Dictionaries;

namespace IRM.Settlements.Api.Contracts.Validation;

public class GetServiceCentersRequestValidator : AbstractValidator<GetServiceCentersRequest>
{
    public GetServiceCentersRequestValidator()
    {
        RuleFor(x => x.ServiceCompanySapId)
            .NotNull()
            .NotEmpty()
            .WithMessage("ServiceCompanySapId is required");
    }
}
