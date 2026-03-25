using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace AbbaFleet.Features.Auth;

public class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);

        if (result.IsLockedOut || result.IsNotAllowed || !result.Succeeded)
        {
            return new LoginResult(false);
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is not null && !user.IsActive)
        {
            await signInManager.SignOutAsync();

            return new LoginResult(false, IsDeactivated: true);
        }

        if (user is not null)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);
        }

        return new LoginResult(true);
    }
}
