using FluentValidation;

namespace AbbaFleet.Features.Clients;

public class ClientValidator : AbstractValidator<UpsertClientRequest>
{
    public ClientValidator()
    {
        RuleFor(r => r.CompanyName).NotEmpty().MaximumLength(100);
        RuleFor(r => r.Description).MaximumLength(500).When(r => r.Description is not null);
        RuleFor(r => r.Address).MaximumLength(200).When(r => r.Address is not null);
        RuleFor(r => r.TaxRate).InclusiveBetween(0m, 100m);
    }
}
