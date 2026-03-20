namespace AbbaFleet.Shared;

public interface IPermissionService
{
    Task<bool> HasAsync(Permission permission);
}
