namespace AbbaFleet.Shared;

public static class PermissionClaimTypes
{
    public const string Permission = "Permission";
}

public enum Permission
{
    DashboardAccess,
    SubmitTrips,
    VerifyTrips,
    SubmitExpenses,
    AccessReports,
    ManageTrucks,
    ManageDrivers,
    ManageCodes,
    ManageCompanies,
    ReconcilePayments,
    UploadRemittanceProof,
    ManageWithdrawals,
    ViewFileRepository,
    ManageUsers
}
