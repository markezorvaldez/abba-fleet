namespace AbbaFleet.Features.Drivers;

public interface IDriverRepository
{
    Task<IReadOnlyList<Driver>> GetAllAsync();
    Task<Driver?> GetByIdAsync(Guid id);
    Task AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task DeleteAsync(Driver driver);
}
