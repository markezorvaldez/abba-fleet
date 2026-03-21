namespace AbbaFleet.Features.Drivers;

public interface IDriverRepository
{
    Task<IReadOnlyList<Driver>> GetAllAsync();
    Task AddAsync(Driver driver);
}
