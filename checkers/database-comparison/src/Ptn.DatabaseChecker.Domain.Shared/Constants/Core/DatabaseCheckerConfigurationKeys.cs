namespace Ptn.DatabaseChecker.Constants;

// islevi: Birden cok altyapi katmaninin okudugu uygulama configuration yollarini tanimlar.
// sistemdeki gorevi: Host ve EFCore modul kurulumlarinda ayni section adlarinin magic string olarak dagilmasini engeller.
public static class DatabaseCheckerConfigurationKeys
{
    // ABP/uygulama EF Core sema override'larini tasiyan section.
    public const string EntityFrameworkCoreSchemasSection = "EntityFrameworkCore:Schemas";
    public const string AbpIdentitySchema = "Volo.Abp.Identity";
    public const string AbpPermissionManagementSchema = "Volo.Abp.PermissionManagement";
    public const string AbpSettingManagementSchema = "Volo.Abp.SettingManagement";
    public const string AbpAuditLoggingSchema = "Volo.Abp.AuditLogging";
    public const string AbpBackgroundJobsSchema = "Volo.Abp.BackgroundJobs";
    public const string AbpFeatureManagementSchema = "Volo.Abp.FeatureManagement";
    public const string AbpTenantManagementSchema = "Volo.Abp.TenantManagement";
    public const string AbpOpenIddictSchema = "Volo.Abp.OpenIddict";
    public const string PitonEmailingSchema = "Piton.Emailing";
    public const string LookupsSchema = "Ptn.DatabaseChecker.Lookups";
    public const string ConnectionsSchema = "Ptn.DatabaseChecker.Connections";
    public const string DefinitionsSchema = "Ptn.DatabaseChecker.Definitions";
    public const string RunsSchema = "Ptn.DatabaseChecker.Runs";
    public const string OperatorsSchema = "Ptn.DatabaseChecker.Operators";
    public const string ComparisonSchema = "Ptn.DatabaseChecker.Comparison";
    public const string EmailSchema = "Ptn.DatabaseChecker.Email";
}
