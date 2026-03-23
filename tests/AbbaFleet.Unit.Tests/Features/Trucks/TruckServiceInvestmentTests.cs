using System.Security.Claims;
using AbbaFleet.Features.Trucks;
using AutoFixture;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Trucks;

public class TruckServiceInvestmentTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }

    private readonly ITruckRepository _repository = Substitute.For<ITruckRepository>();
    private readonly IInvestmentRepository _investmentRepository = Substitute.For<IInvestmentRepository>();
    private readonly IValidator<UpsertTruckRequest> _validator = Substitute.For<IValidator<UpsertTruckRequest>>();
    private readonly AuthenticationStateProvider _authStateProvider = Substitute.For<AuthenticationStateProvider>();

    private TruckService CreateService() =>
        new TruckService(_validator, _repository, _investmentRepository, _authStateProvider);

    private void SetupCurrentUser(string userName)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "test");
        var principal = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(principal);
        _authStateProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(state));
    }

    // --- GetInvestmentsAsync ---

    [Fact]
    public async Task GetInvestmentsAsync_ReturnsMappedEntriesAndTotal()
    {
        var truckId = _fixture.Create<Guid>();
        var userName = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();
        var entry1 = new InvestmentEntry(truckId, InvestmentType.Purchase, 100000m, date, "Initial purchase", userName);
        var entry2 = new InvestmentEntry(truckId, InvestmentType.Repair, 2500m, date, "Tyre replacement", userName);
        var entries = new List<InvestmentEntry> { entry1, entry2 };
        var expectedTotal = 102500m;

        _investmentRepository.GetByTruckIdAsync(Arg.Is(truckId)).Returns(entries);
        _investmentRepository.GetTotalByTruckIdAsync(Arg.Is(truckId)).Returns(expectedTotal);

        var service = CreateService();
        var (dtos, total) = await service.GetInvestmentsAsync(truckId);

        Assert.Equal(2, dtos.Count);
        Assert.Equal(entry1.Id, dtos[0].Id);
        Assert.Equal(InvestmentType.Purchase, dtos[0].Type);
        Assert.Equal(100000m, dtos[0].Amount);
        Assert.Equal(entry2.Id, dtos[1].Id);
        Assert.Equal(expectedTotal, total);
    }

    [Fact]
    public async Task GetInvestmentsAsync_NoEntries_ReturnsEmptyListAndZero()
    {
        var truckId = _fixture.Create<Guid>();
        _investmentRepository.GetByTruckIdAsync(Arg.Is(truckId)).Returns(new List<InvestmentEntry>());
        _investmentRepository.GetTotalByTruckIdAsync(Arg.Is(truckId)).Returns(0m);

        var service = CreateService();
        var (dtos, total) = await service.GetInvestmentsAsync(truckId);

        Assert.Empty(dtos);
        Assert.Equal(0m, total);
    }

    // --- AddInvestmentAsync ---

    [Fact]
    public async Task AddInvestmentAsync_ValidRequest_AddsEntryAndReturnsDto()
    {
        var truckId = _fixture.Create<Guid>();
        var userName = _fixture.Create<string>();
        var truck = new Truck(_fixture.Create<string>(), _fixture.Create<string>(),
            OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>());
        var request = new AddInvestmentRequest(InvestmentType.Upgrade, 15000m, _fixture.Create<DateOnly>(), "New engine");

        _repository.GetByIdAsync(Arg.Is(truckId)).Returns((truck, (string?)null));
        SetupCurrentUser(userName);

        var service = CreateService();
        var result = await service.AddInvestmentAsync(truckId, request);

        Assert.True(result.Succeeded);
        Assert.Equal(InvestmentType.Upgrade, result.Value!.Type);
        Assert.Equal(15000m, result.Value.Amount);
        Assert.Equal("New engine", result.Value.Description);
        Assert.Equal(userName, result.Value.CreatedBy);
        await _investmentRepository.Received(1).AddAsync(Arg.Is<InvestmentEntry>(e =>
            e.TruckId == truckId &&
            e.Type == InvestmentType.Upgrade &&
            e.Amount == 15000m));
    }

    [Fact]
    public async Task AddInvestmentAsync_TruckNotFound_ReturnsFailure()
    {
        var truckId = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(truckId)).Returns(((Truck, string?)?)null);

        var service = CreateService();
        var result = await service.AddInvestmentAsync(truckId, new AddInvestmentRequest(
            InvestmentType.Repair, 500m, _fixture.Create<DateOnly>(), null));

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _investmentRepository.DidNotReceive().AddAsync(Arg.Any<InvestmentEntry>());
    }

    [Fact]
    public async Task AddInvestmentAsync_ZeroAmount_ReturnsFailure()
    {
        var truckId = _fixture.Create<Guid>();
        var truck = new Truck(_fixture.Create<string>(), _fixture.Create<string>(),
            OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>());

        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((truck, (string?)null));

        var service = CreateService();
        var result = await service.AddInvestmentAsync(truckId, new AddInvestmentRequest(
            InvestmentType.Repair, 0m, _fixture.Create<DateOnly>(), null));

        Assert.False(result.Succeeded);
        Assert.Contains("greater than zero", result.Error, StringComparison.OrdinalIgnoreCase);
        await _investmentRepository.DidNotReceive().AddAsync(Arg.Any<InvestmentEntry>());
    }

    [Fact]
    public async Task AddInvestmentAsync_NullUser_ReturnsFailure()
    {
        var truckId = _fixture.Create<Guid>();
        var truck = new Truck(_fixture.Create<string>(), _fixture.Create<string>(),
            OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>());

        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((truck, (string?)null));

        // Return an unauthenticated user (no Name claim)
        var identity = new System.Security.Claims.ClaimsIdentity();
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var state = new AuthenticationState(principal);
        _authStateProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(state));

        var service = CreateService();
        var result = await service.AddInvestmentAsync(truckId, new AddInvestmentRequest(
            InvestmentType.Purchase, 50000m, _fixture.Create<DateOnly>(), null));

        Assert.False(result.Succeeded);
        Assert.Contains("current user", result.Error, StringComparison.OrdinalIgnoreCase);
        await _investmentRepository.DidNotReceive().AddAsync(Arg.Any<InvestmentEntry>());
    }
}
