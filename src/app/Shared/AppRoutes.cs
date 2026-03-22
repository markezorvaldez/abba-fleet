namespace AbbaFleet.Shared;

public static class AppRoutes
{
    public const string Login = "/account/login";
    public const string Logout = "/account/logout";
    public const string Dashboard = "/";
    public const string Users = "/users";
    public const string Drivers = "/drivers";
    public const string DriverDetail = "/drivers/{0}";

    public static string DriverDetailFor(Guid id) => string.Format(DriverDetail, id);
}
