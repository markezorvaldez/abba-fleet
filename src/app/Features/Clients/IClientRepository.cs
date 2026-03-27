namespace AbbaFleet.Features.Clients;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync();

    Task<Client?> GetByIdAsync(Guid id);

    Task<Client?> GetByCompanyNameAsync(string companyName);

    Task<IReadOnlyList<ClientAssignedTruckDto>> GetAssignedTrucksAsync(Guid clientId);

    Task AddAsync(Client client);

    Task UpdateAsync(Client client);
}
