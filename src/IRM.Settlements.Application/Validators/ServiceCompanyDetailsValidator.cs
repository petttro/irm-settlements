using FluentValidation;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Validators;

public class ServiceCompanyDetailsValidator : AbstractValidator<ServiceCompanyDetails>
{
    public ServiceCompanyDetailsValidator()
    {
        RuleFor(x => x.SapId)
            .NotEmpty()
            .WithMessage("SapId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.BankName)
            .NotEmpty()
            .WithMessage("BankName is required");

        RuleFor(x => x.BankBic)
            .NotEmpty()
            .WithMessage("BankBic is required");

        RuleFor(x => x.BankSwiftCode)
            .NotEmpty()
            .WithMessage("BankSwiftCode is required");

        RuleFor(x => x.CorrespondentAccount)
            .NotEmpty()
            .WithMessage("CorrespondentAccount is required");

        RuleFor(x => x.BankAccount)
            .NotEmpty()
            .WithMessage("BankAccount is required");

        RuleFor(x => x.Inn)
            .NotEmpty()
            .WithMessage("Inn is required");

        RuleFor(x => x.Kpp)
            .NotEmpty()
            .WithMessage("Kpp is required");
    }
}
