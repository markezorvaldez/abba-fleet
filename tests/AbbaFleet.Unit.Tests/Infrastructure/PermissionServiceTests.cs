using System.Security.Claims;
using AbbaFleet.Infrastructure;
using AbbaFleet.Shared;
using AutoFixture;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Infrastructure;

public class PermissionServiceTests
{
    private readonly IFixture _fixture = new Fixture();

    /// <summary>
    ///     A controllable TimeProvider that returns a configurable UTC time.
    /// </summary>
    private sealed class FakeTimeProvider : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return UtcNow;
        }
    }

    private static AuthenticationStateProvider BuildUnauthenticatedAuthProvider()
    {
        var provider = Substitute.For<AuthenticationStateProvider>();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        provider.GetAuthenticationStateAsync()
                .Returns(Task.FromResult(new AuthenticationState(anonymous)));

        return provider;
    }

    private static PermissionService BuildService(
        AuthenticationStateProvider authProvider,
        TimeProvider timeProvider)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();

        return new PermissionService(serviceProvider, authProvider, timeProvider);
    }

    [Fact]
    public async Task HasAsync_AfterTtlExpires_ReloadsPermissions()
    {
        var timeProvider = new FakeTimeProvider { UtcNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero) };
        var authProvider = BuildUnauthenticatedAuthProvider();
        var service = BuildService(authProvider, timeProvider);

        // First call — triggers load
        await service.HasAsync(Permission.DashboardAccess);

        // Advance time past the 5-minute TTL
        timeProvider.UtcNow = timeProvider.UtcNow.AddMinutes(5).AddSeconds(1);

        // Second call — cache expired, should reload
        await service.HasAsync(Permission.DashboardAccess);

        // LoadPermissionsAsync called twice (two GetAuthenticationStateAsync calls)
        await authProvider.Received(2).GetAuthenticationStateAsync();
    }

    [Fact]
    public async Task HasAsync_AtExactTtlBoundary_DoesNotReload()
    {
        var timeProvider = new FakeTimeProvider { UtcNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero) };
        var authProvider = BuildUnauthenticatedAuthProvider();
        var service = BuildService(authProvider, timeProvider);

        // First call — triggers load
        await service.HasAsync(Permission.DashboardAccess);

        // Advance time by exactly 5 minutes (not yet expired — TTL is strictly greater than)
        timeProvider.UtcNow = timeProvider.UtcNow.AddMinutes(5);

        // Second call — exactly at TTL boundary, should still use cache
        await service.HasAsync(Permission.DashboardAccess);

        await authProvider.Received(1).GetAuthenticationStateAsync();
    }

    [Fact]
    public async Task HasAsync_MultipleCallsWithinTtl_AllReuseSameCache()
    {
        var timeProvider = new FakeTimeProvider { UtcNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero) };
        var authProvider = BuildUnauthenticatedAuthProvider();
        var service = BuildService(authProvider, timeProvider);

        // Several calls without advancing time
        await service.HasAsync(Permission.DashboardAccess);
        await service.HasAsync(Permission.ManageDrivers);
        await service.HasAsync(Permission.ManageTrucks);

        // Only one load ever performed
        await authProvider.Received(1).GetAuthenticationStateAsync();
    }

    [Fact]
    public async Task HasAsync_TtlExpiresAndCacheReloads_ThenFreshTtlApplies()
    {
        var timeProvider = new FakeTimeProvider { UtcNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero) };
        var authProvider = BuildUnauthenticatedAuthProvider();
        var service = BuildService(authProvider, timeProvider);

        // First load
        await service.HasAsync(Permission.DashboardAccess);

        // Expire TTL → second load
        timeProvider.UtcNow = timeProvider.UtcNow.AddMinutes(5).AddSeconds(1);
        await service.HasAsync(Permission.DashboardAccess);

        // Advance 4 minutes from second load — still within new TTL
        timeProvider.UtcNow = timeProvider.UtcNow.AddMinutes(4);
        await service.HasAsync(Permission.DashboardAccess);

        // Should have loaded exactly twice: initial + after first expiry
        await authProvider.Received(2).GetAuthenticationStateAsync();
    }

    [Fact]
    public async Task HasAsync_WithinTtl_DoesNotReloadPermissions()
    {
        var timeProvider = new FakeTimeProvider { UtcNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero) };
        var authProvider = BuildUnauthenticatedAuthProvider();
        var service = BuildService(authProvider, timeProvider);

        // First call — triggers load
        await service.HasAsync(Permission.DashboardAccess);

        // Advance time by 4 minutes (within TTL)
        timeProvider.UtcNow = timeProvider.UtcNow.AddMinutes(4);

        // Second call — should reuse cache
        await service.HasAsync(Permission.DashboardAccess);

        // LoadPermissionsAsync only called once (one GetAuthenticationStateAsync call)
        await authProvider.Received(1).GetAuthenticationStateAsync();
    }
}
