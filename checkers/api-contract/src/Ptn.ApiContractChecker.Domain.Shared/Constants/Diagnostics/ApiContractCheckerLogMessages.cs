namespace Ptn.ApiContractChecker.Constants.Diagnostics;

// islevi: Uygulamanin yapilandirilmis log sablonlarini kararli adlarla tanimlar.
// sistemdeki gorevi: Domain ve host log olaylarinin metin/placeholder sozlesmesini tek kaynaktan korur.
public static class ApiContractCheckerLogMessages
{
    public const string ExistingTenantBackfillStarted = "Backfilling Api Contract Checker roles and permissions for {TenantCount} existing tenants.";
    public const string ExistingTenantBackfillTenantStarted = "Backfilling Api Contract Checker roles and permissions for tenant {TenantId}.";
    public const string StartingWebHost = "Starting web host.";
    public const string HostTerminatedUnexpectedly = "Host terminated unexpectedly!";
}
