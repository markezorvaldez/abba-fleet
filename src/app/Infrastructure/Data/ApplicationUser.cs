using Microsoft.AspNetCore.Identity;

namespace AbbaFleet.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
