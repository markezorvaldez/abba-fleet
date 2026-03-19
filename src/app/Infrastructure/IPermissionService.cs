namespace AbbaFleet.Infrastructure;

public interface IPermissionService
{
    Task<bool> HasAsync(Permission permission);
}
