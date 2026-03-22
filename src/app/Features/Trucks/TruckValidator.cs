using FluentValidation;

namespace AbbaFleet.Features.Trucks;

public class TruckValidator : AbstractValidator<UpsertTruckRequest>
{
    public TruckValidator()
    {
        RuleFor(r => r.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(r => r.TruckModel).NotEmpty().MaximumLength(100);
    }
}
