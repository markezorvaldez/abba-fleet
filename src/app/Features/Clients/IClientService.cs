using AbbaFleet.Shared;

namespace AbbaFleet.Features.Clients;

public interface IClientService
{
    Task<IReadOnlyList<ClientSummary>> GetAllAsync();

    Task<ClientDetailDto?> GetByIdAsync(Guid id);

    Task<Result<ClientDetailDto>> CreateAsync(UpsertClientRequest request);

    Task<Result<ClientDetailDto>> UpdateAsync(Guid id, UpsertClientRequest request);

    Task<bool> HasUnlockedReconciliationsAsync(Guid clientId);
}
