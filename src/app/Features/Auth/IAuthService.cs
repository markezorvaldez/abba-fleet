namespace AbbaFleet.Features.Auth;

public sealed record LoginResult(bool Succeeded, bool IsDeactivated = false);

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
}
