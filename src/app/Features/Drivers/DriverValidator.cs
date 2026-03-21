using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class DriverValidator : AbstractValidator<Driver>
{
    public DriverValidator()
    {
        RuleFor(d => d.FullName).NotEmpty().MaximumLength(100);
        RuleFor(d => d.PhoneNumber).NotEmpty().MaximumLength(100);
        RuleFor(d => d.FacebookLink).MaximumLength(100).When(d => d.FacebookLink is not null);
        RuleFor(d => d.Address).MaximumLength(100).When(d => d.Address is not null);
    }
}
