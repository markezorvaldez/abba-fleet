namespace AbbaFleet.Features.Clients;

public interface IContactRepository
{
    Task<IReadOnlyList<Contact>> GetByClientIdAsync(Guid clientId);

    Task<Contact?> GetByIdAsync(Guid id);

    Task AddAsync(Contact contact);

    Task UpdateAsync(Contact contact);

    Task DeleteAsync(Contact contact);
}
