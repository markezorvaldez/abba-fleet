using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using Xunit;

namespace AbbaFleet.UnitTests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void NewUser_HasNoPermissions()
    {
        var user = new ApplicationUser();
        Assert.Empty(user.Permissions);
    }

    [Fact]
    public void NewUser_IsActiveByDefault()
    {
        var user = new ApplicationUser();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void HasPermission_GrantedPermission_ReturnsTrue()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.DashboardAccess);
        Assert.True(user.HasPermission(Permission.DashboardAccess));
    }

    [Fact]
    public void HasPermission_NotGrantedPermission_ReturnsFalse()
    {
        var user = new ApplicationUser();
        Assert.False(user.HasPermission(Permission.ManageUsers));
    }

    [Fact]
    public void Grant_AddsPermission()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.SubmitTrips);
        Assert.Contains(Permission.SubmitTrips, user.Permissions);
    }

    [Fact]
    public void Grant_SamePermissionTwice_NoDuplicates()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.SubmitTrips);
        user.Grant(Permission.SubmitTrips);
        Assert.Equal(1, user.Permissions.Count(p => p == Permission.SubmitTrips));
    }

    [Fact]
    public void Revoke_RemovesPermission()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.SubmitTrips);
        user.Revoke(Permission.SubmitTrips);
        Assert.DoesNotContain(Permission.SubmitTrips, user.Permissions);
    }

    [Fact]
    public void Revoke_NonExistentPermission_DoesNotThrow()
    {
        var user = new ApplicationUser();
        var ex = Record.Exception(() => user.Revoke(Permission.ManageUsers));
        Assert.Null(ex);
    }

    [Fact]
    public void SetPermissions_ReplacesAllPermissions()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.SubmitTrips);
        user.SetPermissions([Permission.DashboardAccess, Permission.ManageUsers]);
        Assert.Equal(2, user.Permissions.Count);
        Assert.Contains(Permission.DashboardAccess, user.Permissions);
        Assert.Contains(Permission.ManageUsers, user.Permissions);
        Assert.DoesNotContain(Permission.SubmitTrips, user.Permissions);
    }

    [Fact]
    public void GrantAll_GrantsAll14Permissions()
    {
        var user = new ApplicationUser();
        user.GrantAll();
        var allPermissions = Enum.GetValues<Permission>();
        Assert.Equal(allPermissions.Length, user.Permissions.Count);
        Assert.All(allPermissions, p => Assert.Contains(p, user.Permissions));
    }

    [Fact]
    public void HasAllPermissions_AllPresent_ReturnsTrue()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.DashboardAccess);
        user.Grant(Permission.SubmitTrips);
        Assert.True(user.HasAllPermissions(Permission.DashboardAccess, Permission.SubmitTrips));
    }

    [Fact]
    public void HasAllPermissions_SomeMissing_ReturnsFalse()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.DashboardAccess);
        Assert.False(user.HasAllPermissions(Permission.DashboardAccess, Permission.SubmitTrips));
    }

    [Fact]
    public void HasAllPermissions_NonePresent_ReturnsFalse()
    {
        var user = new ApplicationUser();
        Assert.False(user.HasAllPermissions(Permission.DashboardAccess, Permission.SubmitTrips));
    }

    [Fact]
    public void HasAnyPermission_OnePresent_ReturnsTrue()
    {
        var user = new ApplicationUser();
        user.Grant(Permission.DashboardAccess);
        Assert.True(user.HasAnyPermission(Permission.DashboardAccess, Permission.ManageUsers));
    }

    [Fact]
    public void HasAnyPermission_NonePresent_ReturnsFalse()
    {
        var user = new ApplicationUser();
        Assert.False(user.HasAnyPermission(Permission.DashboardAccess, Permission.ManageUsers));
    }

    [Fact]
    public void Permissions_ExposedAsReadOnlySet()
    {
        var user = new ApplicationUser();
        Assert.IsAssignableFrom<IReadOnlySet<Permission>>(user.Permissions);
    }
}
