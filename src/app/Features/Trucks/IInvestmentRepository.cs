namespace AbbaFleet.Features.Trucks;

public interface IInvestmentRepository
{
    Task<IReadOnlyList<InvestmentEntry>> GetByTruckIdAsync(Guid truckId);

    Task AddAsync(InvestmentEntry entry);

    Task<decimal> GetTotalByTruckIdAsync(Guid truckId);
}
