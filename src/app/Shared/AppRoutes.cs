namespace AbbaFleet.Shared;

public static class AppRoutes
{
    public const string Login = "/account/login";
    public const string Logout = "/account/logout";
    public const string Dashboard = "/";
    public const string Users = "/users";
    public const string Drivers = "/drivers";
    public const string DriverDetail = "/drivers/{0}";
    public const string Trucks = "/trucks";
    public const string TruckDetail = "/trucks/{0}";
    public const string Clients = "/clients";
    public const string ClientDetail = "/clients/{0}";

    public static string DriverDetailFor(Guid id)
    {
        return string.Format(DriverDetail, id);
    }

    public static string TruckDetailFor(Guid id)
    {
        return string.Format(TruckDetail, id);
    }

    public static string ClientDetailFor(Guid id)
    {
        return string.Format(ClientDetail, id);
    }
}
