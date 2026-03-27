using AbbaFleet.Features.Clients;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Clients;

public class ClientServiceTests
{
    private readonly IClientRepository _repository = Substitute.For<IClientRepository>();
    private readonly IValidator<UpsertClientRequest> _validator = Substitute.For<IValidator<UpsertClientRequest>>();
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _sut = new ClientService(_validator, _repository);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }

    private static Client MakeClient(string name = "ACME Corp")
    {
        return new Client(name, "A description", "123 Main St", 12.5m);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCompanyName_ReturnsFailure()
    {
        var request = new UpsertClientRequest("Existing Co", null, null, 0m);
        _validator.ValidateAsync(Arg.Any<UpsertClientRequest>()).Returns(new ValidationResult());
        _repository.GetByCompanyNameAsync("Existing Co").Returns(MakeClient("Existing Co"));

        var result = await _sut.CreateAsync(request);

        Assert.False(result.Succeeded);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Client>());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_AddsAndReturnsDto()
    {
        var fixture = CreateFixture();

        var request = fixture.Build<UpsertClientRequest>()
                             .With(r => r.CompanyName, "New Co")
                             .With(r => r.TaxRate, 10m)
                             .Create();

        _validator.ValidateAsync(Arg.Is<UpsertClientRequest>(r => r.CompanyName == request.CompanyName))
                  .Returns(new ValidationResult());

        _repository.GetByCompanyNameAsync("New Co").Returns((Client?)null);
        _repository.GetAssignedTrucksAsync(Arg.Any<Guid>()).Returns([]);

        var result = await _sut.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("New Co", result.Value!.CompanyName);
        await _repository.Received(1).AddAsync(Arg.Is<Client>(c => c.CompanyName == "New Co"));
    }

    [Fact]
    public async Task CreateAsync_ValidationFails_ReturnsFailure()
    {
        var request = new UpsertClientRequest("", null, null, 0m);
        var errors = new[] { new ValidationFailure("CompanyName", "Required") };
        _validator.ValidateAsync(Arg.Any<UpsertClientRequest>()).Returns(new ValidationResult(errors));

        var result = await _sut.CreateAsync(request);

        Assert.False(result.Succeeded);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Client>());
    }

    // --- HasUnlockedReconciliationsAsync ---

    [Fact]
    public async Task HasUnlockedReconciliationsAsync_AlwaysReturnsFalse()
    {
        var result = await _sut.HasUnlockedReconciliationsAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ClientNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var request = new UpsertClientRequest("X", null, null, 0m);

        _validator.ValidateAsync(Arg.Any<UpsertClientRequest>()).Returns(new ValidationResult());
        _repository.GetByIdAsync(id).Returns((Client?)null);

        var result = await _sut.UpdateAsync(id, request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateNameOnOtherClient_ReturnsFailure()
    {
        var client = MakeClient("Client A");
        var otherClient = MakeClient("Client B");
        var request = new UpsertClientRequest("Client B", null, null, 0m);

        _validator.ValidateAsync(Arg.Any<UpsertClientRequest>()).Returns(new ValidationResult());
        _repository.GetByIdAsync(client.Id).Returns(client);
        _repository.GetByCompanyNameAsync("Client B").Returns(otherClient);

        var result = await _sut.UpdateAsync(client.Id, request);

        Assert.False(result.Succeeded);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Client>());
    }

    [Fact]
    public async Task UpdateAsync_SameNameSameClient_Succeeds()
    {
        var client = MakeClient("ACME Corp");
        var request = new UpsertClientRequest("ACME Corp", null, null, 10m);

        _validator.ValidateAsync(Arg.Any<UpsertClientRequest>()).Returns(new ValidationResult());
        _repository.GetByIdAsync(client.Id).Returns(client);

        // uniqueness check returns the SAME client — this is fine
        _repository.GetByCompanyNameAsync("ACME Corp").Returns(client);
        _repository.GetAssignedTrucksAsync(client.Id).Returns([]);

        var result = await _sut.UpdateAsync(client.Id, request);

        Assert.True(result.Succeeded);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesAndReturnsDto()
    {
        var client = MakeClient("Old Name");
        var request = new UpsertClientRequest("New Name", null, null, 5m);

        _validator.ValidateAsync(Arg.Is<UpsertClientRequest>(r => r.CompanyName == "New Name"))
                  .Returns(new ValidationResult());

        _repository.GetByIdAsync(client.Id).Returns(client);
        _repository.GetByCompanyNameAsync("New Name").Returns((Client?)null);
        _repository.GetAssignedTrucksAsync(client.Id).Returns([]);

        var result = await _sut.UpdateAsync(client.Id, request);

        Assert.True(result.Succeeded);
        Assert.Equal("New Name", result.Value!.CompanyName);
        await _repository.Received(1).UpdateAsync(Arg.Is<Client>(c => c.Id == client.Id));
    }
}
