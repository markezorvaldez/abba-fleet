using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Clients;

public class ClientService(
    IValidator<UpsertClientRequest> validator,
    IClientRepository repository) : IClientService
{
    public async Task<IReadOnlyList<ClientSummary>> GetAllAsync()
    {
        var clients = await repository.GetAllAsync();

        return clients
               .Select(c => new ClientSummary(c.Id, c.CompanyName, c.Address, c.TaxRate, c.IsActive))
               .ToList();
    }

    public async Task<ClientDetailDto?> GetByIdAsync(Guid id)
    {
        var client = await repository.GetByIdAsync(id);

        if (client is null)
        {
            return null;
        }

        var trucks = await repository.GetAssignedTrucksAsync(id);

        return MapToDetail(client, trucks);
    }

    public async Task<Result<ClientDetailDto>> CreateAsync(UpsertClientRequest request)
    {
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        var existing = await repository.GetByCompanyNameAsync(request.CompanyName.Trim());

        if (existing is not null)
        {
            return "A client with this company name already exists.";
        }

        var client = new Client(request.CompanyName, request.Description, request.Address, request.TaxRate);
        await repository.AddAsync(client);

        var trucks = await repository.GetAssignedTrucksAsync(client.Id);

        return MapToDetail(client, trucks);
    }

    public async Task<Result<ClientDetailDto>> UpdateAsync(Guid id, UpsertClientRequest request)
    {
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        var client = await repository.GetByIdAsync(id);

        if (client is null)
        {
            return "Client not found.";
        }

        var duplicate = await repository.GetByCompanyNameAsync(request.CompanyName.Trim());

        if (duplicate is not null && duplicate.Id != id)
        {
            return "A client with this company name already exists.";
        }

        client.Update(request.CompanyName, request.Description, request.Address, request.TaxRate, request.IsActive);
        await repository.UpdateAsync(client);

        var trucks = await repository.GetAssignedTrucksAsync(id);

        return MapToDetail(client, trucks);
    }

    public Task<bool> HasUnlockedReconciliationsAsync(Guid clientId)
    {
        // TODO: return true when Reconciliation feature is built and has unlocked records for this client
        return Task.FromResult(false);
    }

    private static ClientDetailDto MapToDetail(Client client, IReadOnlyList<ClientAssignedTruckDto> trucks)
    {
        return new ClientDetailDto(
            client.Id,
            client.CompanyName,
            client.Description,
            client.Address,
            client.TaxRate,
            client.IsActive,
            client.CreatedAt,
            client.UpdatedAt,
            trucks);
    }
}
