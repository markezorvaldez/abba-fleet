using System.Security.Claims;
using AbbaFleet.Features.Users;
using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Users;

public class UserServiceTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task ToggleActiveAsync_LastActiveAdmin_ReturnsFailure()
    {
        var userManager = CreateUserManager();
        var userId = "admin-1";
        var admin = new ApplicationUser { Id = userId, FullName = "Admin", IsActive = true };

        userManager.FindByIdAsync(userId).Returns(admin);
        userManager.GetUsersForClaimAsync(Arg.Any<Claim>()).Returns([admin]);

        var service = new UserService(userManager);
        var result = await service.ToggleActiveAsync(userId, makeActive: false);

        Assert.False(result.Succeeded);
        Assert.Contains("last admin", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenAnotherAdminExists_Succeeds()
    {
        var userManager = CreateUserManager();
        var userId = "admin-1";
        var admin1 = new ApplicationUser { Id = userId, FullName = "Admin 1", IsActive = true };
        var admin2 = new ApplicationUser { Id = "admin-2", FullName = "Admin 2", IsActive = true };

        userManager.FindByIdAsync(userId).Returns(admin1);
        userManager.GetUsersForClaimAsync(Arg.Any<Claim>()).Returns([admin1, admin2]);
        userManager.UpdateAsync(admin1).Returns(IdentityResult.Success);

        var service = new UserService(userManager);
        var result = await service.ToggleActiveAsync(userId, makeActive: false);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task UpdateAsync_WhenEditingOwnAccount_ManageUsersPermissionIsPreserved()
    {
        var userManager = CreateUserManager();
        var userId = "admin-1";
        var user = new ApplicationUser { Id = userId, FullName = "Admin", IsActive = true };

        userManager.FindByIdAsync(userId).Returns(user);
        userManager.GetClaimsAsync(user).Returns(
            [new Claim(PermissionClaimTypes.Permission, Permission.ManageUsers.ToString())]);
        userManager.UpdateAsync(user).Returns(IdentityResult.Success);

        var service = new UserService(userManager);

        // Pass empty permissions — self-protection should re-add ManageUsers
        var result = await service.UpdateAsync(
            userId,
            fullName: "Admin",
            isActive: true,
            newPassword: null,
            targetPermissions: new HashSet<Permission>(),
            currentUserId: userId);

        Assert.True(result.Succeeded);

        // ManageUsers should never have been removed
        await userManager.DidNotReceive().RemoveClaimAsync(
            user,
            Arg.Is<Claim>(c =>
                c.Type == PermissionClaimTypes.Permission &&
                c.Value == Permission.ManageUsers.ToString()));
    }
}
