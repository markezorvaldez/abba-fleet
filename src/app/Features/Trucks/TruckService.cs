using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Trucks;

public class TruckService(IValidator<UpsertTruckRequest> validator, ITruckRepository repository) : ITruckService
{
    public async Task<IReadOnlyList<TruckSummary>> GetAllAsync()
    {
        var trucks = await repository.GetAllAsync();
        return trucks
            .Select(t => new TruckSummary(
                t.Truck.Id,
                t.Truck.PlateNumber,
                t.Truck.TruckModel,
                t.Truck.OwnershipType,
                t.DriverName,
                t.Truck.IsActive))
            .ToList();
    }

    public async Task<TruckDetailDto?> GetByIdAsync(Guid id)
    {
        var result = await repository.GetByIdAsync(id);

        if (result is null)
        {
            return null;
        }

        return MapToDetail(result.Value.Truck, result.Value.DriverName);
    }

    public async Task<Result<Truck>> CreateAsync(UpsertTruckRequest request)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        if (await repository.ExistsWithPlateNumberAsync(request.PlateNumber))
        {
            return "A truck with this plate number already exists.";
        }

        var truck = new Truck(
            request.PlateNumber,
            request.TruckModel,
            request.OwnershipType,
            request.DriverId,
            request.DateAcquired);

        await repository.AddAsync(truck);
        return truck;
    }

    public async Task<Result<TruckDetailDto>> UpdateAsync(Guid id, UpsertTruckRequest request)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        var result = await repository.GetByIdAsync(id);

        if (result is null)
        {
            return "Truck not found.";
        }

        if (await repository.ExistsWithPlateNumberAsync(request.PlateNumber, id))
        {
            return "A truck with this plate number already exists.";
        }

        var truck = result.Value.Truck;
        truck.Update(
            request.PlateNumber,
            request.TruckModel,
            request.OwnershipType,
            request.DriverId,
            request.IsActive,
            request.DateAcquired);

        await repository.UpdateAsync(truck);

        // Re-fetch to get updated driver name
        var updated = await repository.GetByIdAsync(id);
        return MapToDetail(updated!.Value.Truck, updated.Value.DriverName);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var result = await repository.GetByIdAsync(id);

        if (result is null)
        {
            return "Truck not found.";
        }

        // TODO: When Trip/Expense entities exist, check for associated records before allowing delete.

        await repository.DeleteAsync(result.Value.Truck);
        return true;
    }

    public Task<IReadOnlyList<LookupItem>> GetDriverOptionsAsync()
    {
        return repository.GetActiveDriverOptionsAsync();
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id, string reason)
    {
        var result = await repository.GetByIdAsync(id);

        if (result is null)
        {
            return "Truck not found.";
        }

        var truck = result.Value.Truck;

        if (!truck.IsActive)
        {
            return "Truck is already inactive.";
        }

        truck.Update(
            truck.PlateNumber,
            truck.TruckModel,
            truck.OwnershipType,
            truck.DriverId,
            isActive: false,
            truck.DateAcquired);

        await repository.UpdateAsync(truck);
        return true;
    }

    public async Task<Result<bool>> ReactivateAsync(Guid id, string reason)
    {
        var result = await repository.GetByIdAsync(id);

        if (result is null)
        {
            return "Truck not found.";
        }

        var truck = result.Value.Truck;

        if (truck.IsActive)
        {
            return "Truck is already active.";
        }

        truck.Update(
            truck.PlateNumber,
            truck.TruckModel,
            truck.OwnershipType,
            truck.DriverId,
            isActive: true,
            truck.DateAcquired);

        await repository.UpdateAsync(truck);
        return true;
    }

    private static TruckDetailDto MapToDetail(Truck t, string? driverName) => new(
        t.Id,
        t.PlateNumber,
        t.TruckModel,
        t.OwnershipType,
        t.DriverId,
        driverName,
        t.IsActive,
        t.DateAcquired,
        t.CreatedAt,
        t.UpdatedAt);
}
