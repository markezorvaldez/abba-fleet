using Microsoft.AspNetCore.Identity;

namespace AbbaFleet.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    private HashSet<Permission> _permissions = [];
    public IReadOnlySet<Permission> Permissions => _permissions;

    public bool HasPermission(Permission p) => _permissions.Contains(p);
    public bool HasAllPermissions(params Permission[] ps) => ps.All(p => _permissions.Contains(p));
    public bool HasAnyPermission(params Permission[] ps) => ps.Any(p => _permissions.Contains(p));
    public void Grant(Permission p) => _permissions.Add(p);
    public void Revoke(Permission p) => _permissions.Remove(p);
    public void SetPermissions(IEnumerable<Permission> ps) => _permissions = ps.ToHashSet();
    public void GrantAll() => _permissions = Enum.GetValues<Permission>().ToHashSet();
}
