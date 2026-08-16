namespace Ptn.ApiContractChecker.Constants;

// islevi: Birden cok altyapi katmaninin okudugu uygulama configuration yollarini tanimlar.
// sistemdeki gorevi: Host ve EFCore modul kurulumlarinda ayni section adlarinin magic string olarak dagilmasini engeller.
public static class ApiContractCheckerConfigurationKeys
{
    // ABP/uygulama EF Core sema override'larini tasiyan section.
    public const string EntityFrameworkCoreSchemasSection = "EntityFrameworkCore:Schemas";
    public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    public const string AppSettingsFileName = "appsettings.json";
    public const string AppSettingsSecretsFileName = "appsettings.secrets.json";
    public const string AppSettingsEnvironmentFileFormat = "appsettings.{0}.json";
    public const string HostDirectoryName = "host";
    public const string HttpApiHostProjectName = "Ptn.ApiContractChecker.HttpApi.Host";
    public const string AppSelfUrl = "App:SelfUrl";
    public const string AppCorsOrigins = "App:CorsOrigins";
    public const string DatabaseAutoMigrate = "Database:AutoMigrate";
    public const string DatabaseSeedOnStartup = "Database:SeedOnStartup";
    public const string AbpIdentitySchema = "Volo.Abp.Identity";
    public const string AbpPermissionManagementSchema = "Volo.Abp.PermissionManagement";
    public const string AbpSettingManagementSchema = "Volo.Abp.SettingManagement";
    public const string AbpAuditLoggingSchema = "Volo.Abp.AuditLogging";
    public const string AbpBackgroundJobsSchema = "Volo.Abp.BackgroundJobs";
    public const string AbpFeatureManagementSchema = "Volo.Abp.FeatureManagement";
    public const string AbpTenantManagementSchema = "Volo.Abp.TenantManagement";
    public const string AbpOpenIddictSchema = "Volo.Abp.OpenIddict";
    public const string PitonEmailingSchema = "Piton.Emailing";
    public const string OperatorsSchema = "Ptn.ApiContractChecker.Operators";
    public const string CheckerSchema = "Ptn.ApiContractChecker";
    public const string EmailSchema = "Ptn.ApiContractChecker.Email";
}
