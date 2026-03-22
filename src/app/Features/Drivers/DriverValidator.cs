using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class DriverValidator : AbstractValidator<UpsertDriverRequest>
{
    public DriverValidator()
    {
        RuleFor(r => r.FullName).NotEmpty().MaximumLength(100);
        RuleFor(r => r.PhoneNumber).NotEmpty().MaximumLength(100);
        RuleFor(r => r.FacebookLink).MaximumLength(100).When(r => r.FacebookLink is not null);
        RuleFor(r => r.Address).MaximumLength(100).When(r => r.Address is not null);
    }
}
