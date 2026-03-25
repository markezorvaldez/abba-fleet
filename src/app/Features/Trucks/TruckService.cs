using System.Security.Claims;
using AbbaFleet.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;

namespace AbbaFleet.Features.Trucks;

public class TruckService(
    IValidator<UpsertTruckRequest> validator,
    ITruckRepository repository,
    IInvestmentRepository investmentRepository,
    IFileRepository fileRepository,
    IFileStorageService fileStorageService,
    AuthenticationStateProvider authStateProvider) : ITruckService
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

        return result is null ? null : MapToDetail(result.Value.Truck, result.Value.DriverName);
    }

    public async Task<Result<Truck>> CreateAsync(UpsertTruckRequest request, bool forceDriverAssignment = false)
    {
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        if (await repository.ExistsWithPlateNumberAsync(request.PlateNumber))
        {
            return "A truck with this plate number already exists.";
        }

        if (request.DriverId.HasValue)
        {
            var conflictResult = await repository.GetByDriverIdAsync(request.DriverId.Value);

            if (conflictResult is not null && !forceDriverAssignment)
            {
                return $"CONFLICT:{conflictResult.Value.Truck.PlateNumber}";
            }

            if (conflictResult is not null && forceDriverAssignment)
            {
                var conflictTruck = conflictResult.Value.Truck;

                conflictTruck.Update(
                    conflictTruck.PlateNumber,
                    conflictTruck.TruckModel,
                    conflictTruck.OwnershipType,
                    null,
                    conflictTruck.IsActive,
                    conflictTruck.DateAcquired);

                await repository.UpdateAsync(conflictTruck);
            }
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

    public async Task<Result<TruckDetailDto>> UpdateAsync(Guid id, UpsertTruckRequest request, bool forceDriverAssignment = false)
    {
        var validation = await validator.ValidateAsync(request);

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

        if (request.DriverId.HasValue)
        {
            var conflictResult = await repository.GetByDriverIdAsync(request.DriverId.Value);

            if (conflictResult is not null && conflictResult.Value.Truck.Id != id && !forceDriverAssignment)
            {
                return $"CONFLICT:{conflictResult.Value.Truck.PlateNumber}";
            }

            if (conflictResult is not null && conflictResult.Value.Truck.Id != id && forceDriverAssignment)
            {
                var conflictTruck = conflictResult.Value.Truck;

                conflictTruck.Update(
                    conflictTruck.PlateNumber,
                    conflictTruck.TruckModel,
                    conflictTruck.OwnershipType,
                    null,
                    conflictTruck.IsActive,
                    conflictTruck.DateAcquired);

                await repository.UpdateAsync(conflictTruck);
            }
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

        var files = await fileRepository.GetByEntityAsync(NoteEntityType.Truck, id);

        foreach (var file in files)
        {
            await fileStorageService.DeleteAsync(file.StoragePath);
            await fileRepository.DeleteAsync(file);
        }

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

    public async Task<Result<TruckDetailDto>> AssignDriverAsync(Guid truckId, Guid driverId, string reason, bool force = false)
    {
        var truckResult = await repository.GetByIdAsync(truckId);

        if (truckResult is null)
        {
            return "Truck not found.";
        }

        var driver = await repository.GetDriverLookupAsync(driverId);

        if (driver is null)
        {
            return "Driver not found.";
        }

        if (!driver.IsActive)
        {
            return "Deactivated drivers cannot be assigned to a truck.";
        }

        if (truckResult.Value.Truck.DriverId == driverId)
        {
            return "This driver is already assigned to this truck.";
        }

        var conflictResult = await repository.GetByDriverIdAsync(driverId);

        if (conflictResult is not null && !force)
        {
            return $"CONFLICT:{conflictResult.Value.Truck.PlateNumber}";
        }

        if (conflictResult is not null && force)
        {
            var conflictTruck = conflictResult.Value.Truck;

            conflictTruck.Update(
                conflictTruck.PlateNumber,
                conflictTruck.TruckModel,
                conflictTruck.OwnershipType,
                null,
                conflictTruck.IsActive,
                conflictTruck.DateAcquired);

            await repository.UpdateAsync(conflictTruck);
        }

        var truck = truckResult.Value.Truck;

        truck.Update(
            truck.PlateNumber,
            truck.TruckModel,
            truck.OwnershipType,
            driverId,
            truck.IsActive,
            truck.DateAcquired);

        await repository.UpdateAsync(truck);

        var updated = await repository.GetByIdAsync(truckId);

        return MapToDetail(updated!.Value.Truck, updated.Value.DriverName);
    }

    public async Task<Result<TruckDetailDto>> UnassignDriverAsync(Guid truckId, string reason)
    {
        var truckResult = await repository.GetByIdAsync(truckId);

        if (truckResult is null)
        {
            return "Truck not found.";
        }

        var truck = truckResult.Value.Truck;

        if (!truck.DriverId.HasValue)
        {
            return "No driver is currently assigned to this truck.";
        }

        truck.Update(
            truck.PlateNumber,
            truck.TruckModel,
            truck.OwnershipType,
            null,
            truck.IsActive,
            truck.DateAcquired);

        await repository.UpdateAsync(truck);

        var updated = await repository.GetByIdAsync(truckId);

        return MapToDetail(updated!.Value.Truck, updated.Value.DriverName);
    }

    public async Task<(IReadOnlyList<InvestmentDto> Entries, decimal Total)> GetInvestmentsAsync(Guid truckId)
    {
        var entries = await investmentRepository.GetByTruckIdAsync(truckId);
        var total = await investmentRepository.GetTotalByTruckIdAsync(truckId);

        return (entries.Select(MapToInvestmentDto).ToList(), total);
    }

    public async Task<Result<InvestmentDto>> AddInvestmentAsync(Guid truckId, AddInvestmentRequest request)
    {
        var truckResult = await repository.GetByIdAsync(truckId);

        if (truckResult is null)
        {
            return "Truck not found.";
        }

        if (request.Amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        var entry = new InvestmentEntry(truckId, request.Type, request.Amount, request.Date, request.Description, userName);
        await investmentRepository.AddAsync(entry);

        return MapToInvestmentDto(entry);
    }

    private async Task<string?> GetCurrentUserNameAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();

        return state.User.FindFirstValue(ClaimTypes.Name);
    }

    private static InvestmentDto MapToInvestmentDto(InvestmentEntry e)
    {
        return new InvestmentDto(
            e.Id,
            e.Type,
            e.Amount,
            e.Date,
            e.Description,
            e.CreatedBy,
            e.CreatedAt);
    }

    private static TruckDetailDto MapToDetail(Truck t, string? driverName)
    {
        return new TruckDetailDto(
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
}
