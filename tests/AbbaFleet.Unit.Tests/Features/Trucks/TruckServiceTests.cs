using AbbaFleet.Features.Trucks;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Trucks;

public class TruckServiceTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }

    private readonly ITruckRepository _repository = Substitute.For<ITruckRepository>();
    private readonly IValidator<UpsertTruckRequest> _validator = Substitute.For<IValidator<UpsertTruckRequest>>();

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccessAndAddsToRepository()
    {
        var request = _fixture.Create<UpsertTruckRequest>();

        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == request.PlateNumber && r.TruckModel == request.TruckModel))
            .Returns(new ValidationResult());
        _repository.ExistsWithPlateNumberAsync(Arg.Is(request.PlateNumber), Arg.Is<Guid?>(x => x == null))
            .Returns(false);

        var service = new TruckService(_validator, _repository);
        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(request.PlateNumber, result.Value!.PlateNumber);
        await _repository.Received(1).AddAsync(Arg.Is<Truck>(t =>
            t.PlateNumber == request.PlateNumber && t.TruckModel == request.TruckModel));
    }

    [Fact]
    public async Task CreateAsync_InvalidInput_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var errorMessage = _fixture.Create<string>();

        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == request.PlateNumber && r.TruckModel == request.TruckModel))
            .Returns(new ValidationResult([new ValidationFailure("PlateNumber", errorMessage)]));

        var service = new TruckService(_validator, _repository);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task CreateAsync_DuplicatePlateNumber_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertTruckRequest>();

        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == request.PlateNumber))
            .Returns(new ValidationResult());
        _repository.ExistsWithPlateNumberAsync(Arg.Is(request.PlateNumber), null)
            .Returns(true);

        var service = new TruckService(_validator, _repository);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Contains("plate number", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Truck>());
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ReturnsMappedSummaries()
    {
        var plate1 = _fixture.Create<string>();
        var model1 = _fixture.Create<string>();
        var plate2 = _fixture.Create<string>();
        var model2 = _fixture.Create<string>();
        var driverName = _fixture.Create<string>();

        var trucks = new List<(Truck, string?)>
        {
            (new Truck(plate1, model1, OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>()), null),
            (new Truck(plate2, model2, OwnershipType.Subcontracted, _fixture.Create<Guid>(), _fixture.Create<DateOnly>()), driverName)
        };
        _repository.GetAllAsync().Returns(trucks);

        var service = new TruckService(_validator, _repository);
        var summaries = await service.GetAllAsync();

        Assert.Equal(2, summaries.Count);
        Assert.Equal(plate1, summaries[0].PlateNumber);
        Assert.Null(summaries[0].DriverName);
        Assert.Equal(plate2, summaries[1].PlateNumber);
        Assert.Equal(driverName, summaries[1].DriverName);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_TruckExists_ReturnsMappedDetail()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, request.DriverId, request.DateAcquired);
        var driverName = _fixture.Create<string>();
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, driverName));

        var service = new TruckService(_validator, _repository);
        var detail = await service.GetByIdAsync(truck.Id);

        Assert.NotNull(detail);
        Assert.Equal(request.PlateNumber, detail.PlateNumber);
        Assert.Equal(request.TruckModel, detail.TruckModel);
        Assert.Equal(driverName, detail.DriverName);
    }

    [Fact]
    public async Task GetByIdAsync_TruckNotFound_ReturnsNull()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var detail = await service.GetByIdAsync(id);

        Assert.Null(detail);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidInput_ReturnsSuccessAndCallsRepository()
    {
        var createRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            createRequest.PlateNumber, createRequest.TruckModel,
            createRequest.OwnershipType, null, createRequest.DateAcquired);
        var driverName = _fixture.Create<string>();
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, driverName));

        var updateRequest = _fixture.Create<UpsertTruckRequest>();
        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == updateRequest.PlateNumber && r.TruckModel == updateRequest.TruckModel))
            .Returns(new ValidationResult());
        _repository.ExistsWithPlateNumberAsync(Arg.Is(updateRequest.PlateNumber), Arg.Is<Guid?>(truck.Id))
            .Returns(false);

        var service = new TruckService(_validator, _repository);
        var result = await service.UpdateAsync(truck.Id, updateRequest);

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.PlateNumber == updateRequest.PlateNumber && t.TruckModel == updateRequest.TruckModel));
    }

    [Fact]
    public async Task UpdateAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        var request = _fixture.Create<UpsertTruckRequest>();
        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == request.PlateNumber && r.TruckModel == request.TruckModel))
            .Returns(new ValidationResult());
        _repository.GetByIdAsync(Arg.Is(id)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.UpdateAsync(id, request);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task UpdateAsync_DuplicatePlateNumber_ReturnsFailure()
    {
        var createRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            createRequest.PlateNumber, createRequest.TruckModel,
            createRequest.OwnershipType, null, createRequest.DateAcquired);
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var updateRequest = _fixture.Create<UpsertTruckRequest>();
        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == updateRequest.PlateNumber))
            .Returns(new ValidationResult());
        _repository.ExistsWithPlateNumberAsync(Arg.Is(updateRequest.PlateNumber), Arg.Is<Guid?>(truck.Id))
            .Returns(true);

        var service = new TruckService(_validator, _repository);
        var result = await service.UpdateAsync(truck.Id, updateRequest);

        Assert.False(result.Succeeded);
        Assert.Contains("plate number", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task UpdateAsync_InvalidInput_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var errorMessage = _fixture.Create<string>();

        _validator.Validate(Arg.Is<UpsertTruckRequest>(r =>
            r.PlateNumber == request.PlateNumber && r.TruckModel == request.TruckModel))
            .Returns(new ValidationResult([new ValidationFailure("PlateNumber", errorMessage)]));

        var service = new TruckService(_validator, _repository);
        var result = await service.UpdateAsync(_fixture.Create<Guid>(), request);

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_TruckExists_ReturnsSuccessAndCallsRepository()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, null, request.DateAcquired);
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var service = new TruckService(_validator, _repository);
        var result = await service.DeleteAsync(truck.Id);

        Assert.True(result.Succeeded);
        await _repository.Received(1).DeleteAsync(Arg.Is<Truck>(t => t.Id == truck.Id));
    }

    [Fact]
    public async Task DeleteAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.DeleteAsync(id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Truck>());
    }

    // --- DeactivateAsync ---

    [Fact]
    public async Task DeactivateAsync_ActiveTruck_DeactivatesAndCallsRepository()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, null, request.DateAcquired);
        var reason = _fixture.Create<string>();
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var service = new TruckService(_validator, _repository);
        var result = await service.DeactivateAsync(truck.Id, reason);

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == truck.Id && !t.IsActive));
    }

    [Fact]
    public async Task DeactivateAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.DeactivateAsync(id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, null, request.DateAcquired);
        truck.Update(truck.PlateNumber, truck.TruckModel, truck.OwnershipType, truck.DriverId, isActive: false, truck.DateAcquired);
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var service = new TruckService(_validator, _repository);
        var result = await service.DeactivateAsync(truck.Id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("already inactive", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    // --- ReactivateAsync ---

    [Fact]
    public async Task ReactivateAsync_InactiveTruck_ReactivatesAndCallsRepository()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, null, request.DateAcquired);
        truck.Update(truck.PlateNumber, truck.TruckModel, truck.OwnershipType, truck.DriverId, isActive: false, truck.DateAcquired);
        var reason = _fixture.Create<string>();
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var service = new TruckService(_validator, _repository);
        var result = await service.ReactivateAsync(truck.Id, reason);

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == truck.Id && t.IsActive));
    }

    [Fact]
    public async Task ReactivateAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.ReactivateAsync(id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task ReactivateAsync_AlreadyActive_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(
            request.PlateNumber, request.TruckModel,
            request.OwnershipType, null, request.DateAcquired);
        // Truck is active by default after construction
        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, null));

        var service = new TruckService(_validator, _repository);
        var result = await service.ReactivateAsync(truck.Id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("already active", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    // --- AssignDriverAsync ---

    [Fact]
    public async Task AssignDriverAsync_NoConflict_AssignsDriverAndReturnsSuccess()
    {
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, null, truckRequest.DateAcquired);

        var driverId = _fixture.Create<Guid>();
        var driverName = _fixture.Create<string>();
        var driverLookup = new DriverLookup(driverId, driverName, IsActive: true);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, (string?)null), (truck, driverName));
        _repository.GetDriverLookupAsync(Arg.Is(driverId)).Returns(driverLookup);
        _repository.GetByDriverIdAsync(Arg.Is(driverId)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(truck.Id, driverId, "Assigning driver", force: false);

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == truck.Id && t.DriverId == driverId));
    }

    [Fact]
    public async Task AssignDriverAsync_DriverAlreadyAssignedElsewhere_ReturnsConflictError()
    {
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, null, truckRequest.DateAcquired);

        var driverId = _fixture.Create<Guid>();
        var driverLookup = new DriverLookup(driverId, _fixture.Create<string>(), IsActive: true);
        var conflictPlate = _fixture.Create<string>();
        var conflictTruckRequest = _fixture.Create<UpsertTruckRequest>();
        var conflictTruck = new Truck(conflictPlate, conflictTruckRequest.TruckModel,
            conflictTruckRequest.OwnershipType, driverId, conflictTruckRequest.DateAcquired);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, (string?)null));
        _repository.GetDriverLookupAsync(Arg.Is(driverId)).Returns(driverLookup);
        _repository.GetByDriverIdAsync(Arg.Is(driverId)).Returns((conflictTruck, (string?)null));

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(truck.Id, driverId, "Assigning driver", force: false);

        Assert.False(result.Succeeded);
        Assert.StartsWith("CONFLICT:", result.Error);
        Assert.Contains(conflictPlate, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task AssignDriverAsync_ForceReassign_ClearsOldTruckAndAssignsToNew()
    {
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, null, truckRequest.DateAcquired);

        var driverId = _fixture.Create<Guid>();
        var driverName = _fixture.Create<string>();
        var driverLookup = new DriverLookup(driverId, driverName, IsActive: true);
        var conflictTruckRequest = _fixture.Create<UpsertTruckRequest>();
        var conflictTruck = new Truck(conflictTruckRequest.PlateNumber, conflictTruckRequest.TruckModel,
            conflictTruckRequest.OwnershipType, driverId, conflictTruckRequest.DateAcquired);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, (string?)null), (truck, driverName));
        _repository.GetDriverLookupAsync(Arg.Is(driverId)).Returns(driverLookup);
        _repository.GetByDriverIdAsync(Arg.Is(driverId)).Returns((conflictTruck, (string?)null));

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(truck.Id, driverId, "Reassigning", force: true);

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == conflictTruck.Id && t.DriverId == null));
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == truck.Id && t.DriverId == driverId));
    }

    [Fact]
    public async Task AssignDriverAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(id, _fixture.Create<Guid>(), "reason");

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task AssignDriverAsync_DeactivatedDriver_ReturnsFailure()
    {
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, null, truckRequest.DateAcquired);

        var driverId = _fixture.Create<Guid>();
        var driverLookup = new DriverLookup(driverId, _fixture.Create<string>(), IsActive: false);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, (string?)null));
        _repository.GetDriverLookupAsync(Arg.Is(driverId)).Returns(driverLookup);

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(truck.Id, driverId, "reason");

        Assert.False(result.Succeeded);
        Assert.Contains("Deactivated", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task AssignDriverAsync_DriverAlreadyAssignedToThisTruck_ReturnsFailure()
    {
        var driverId = _fixture.Create<Guid>();
        var driverLookup = new DriverLookup(driverId, _fixture.Create<string>(), IsActive: true);
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, driverId, truckRequest.DateAcquired);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, driverLookup.FullName));
        _repository.GetDriverLookupAsync(Arg.Is(driverId)).Returns(driverLookup);

        var service = new TruckService(_validator, _repository);
        var result = await service.AssignDriverAsync(truck.Id, driverId, "reason");

        Assert.False(result.Succeeded);
        Assert.Contains("already assigned to this truck", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    // --- UnassignDriverAsync ---

    [Fact]
    public async Task UnassignDriverAsync_DriverAssigned_RemovesDriverAndReturnsSuccess()
    {
        var driverId = _fixture.Create<Guid>();
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, driverId, truckRequest.DateAcquired);
        var driverName = _fixture.Create<string>();

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, driverName), (truck, (string?)null));

        var service = new TruckService(_validator, _repository);
        var result = await service.UnassignDriverAsync(truck.Id, "Driver left");

        Assert.True(result.Succeeded);
        await _repository.Received(1).UpdateAsync(Arg.Is<Truck>(t =>
            t.Id == truck.Id && t.DriverId == null));
    }

    [Fact]
    public async Task UnassignDriverAsync_TruckNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns(((Truck, string?)?)null);

        var service = new TruckService(_validator, _repository);
        var result = await service.UnassignDriverAsync(id, "reason");

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }

    [Fact]
    public async Task UnassignDriverAsync_NoDriverAssigned_ReturnsFailure()
    {
        var truckRequest = _fixture.Create<UpsertTruckRequest>();
        var truck = new Truck(truckRequest.PlateNumber, truckRequest.TruckModel,
            truckRequest.OwnershipType, null, truckRequest.DateAcquired);

        _repository.GetByIdAsync(Arg.Is(truck.Id)).Returns((truck, (string?)null));

        var service = new TruckService(_validator, _repository);
        var result = await service.UnassignDriverAsync(truck.Id, "reason");

        Assert.False(result.Succeeded);
        Assert.Contains("No driver", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Truck>());
    }
}
