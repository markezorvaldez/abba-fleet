namespace AbbaFleet.Features.Trucks;

public sealed record InvestmentDto(
    Guid Id,
    InvestmentType Type,
    decimal Amount,
    DateOnly Date,
    string? Description,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public sealed record AddInvestmentRequest(
    InvestmentType Type,
    decimal Amount,
    DateOnly Date,
    string? Description);
