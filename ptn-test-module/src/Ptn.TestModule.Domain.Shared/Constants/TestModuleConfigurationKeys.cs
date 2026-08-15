namespace Ptn.TestModule.Constants;

// islevi: Host configuration bolum, dosya ve anahtar adlarini tek sahipte toplar.
// sistemdeki gorevi: appsettings anahtarlarinin kod icinde literal olarak tekrarlanmasini engeller.
public static class TestModuleConfigurationKeys
{
    public const string EntityFrameworkCoreSchemasSection = "EntityFrameworkCore:Schemas";
    public const string LookupSchema = "Ptn.TestModule.Lookup";
    public const string CatalogSchema = "Ptn.TestModule.Catalog";
    public const string RunSchema = "Ptn.TestModule.Run";

    public const string AuthServerAuthority = "AuthServer:Authority";
    public const string AuthServerAudience = "AuthServer:Audience";
    public const string AuthServerRequireHttpsMetadata = "AuthServer:RequireHttpsMetadata";
    public const string AuthServerSwaggerClientId = "AuthServer:SwaggerClientId";

    public const string CorsOrigins = "App:CorsOrigins";
    public const string BlobStoringFileSystemBasePath = "BlobStoring:FileSystemBasePath";
    public const string RedisConfiguration = "Redis:Configuration";
    public const string DatabaseAutoMigrate = "Database:AutoMigrate";
    public const string DatabaseSeedOnStartup = "Database:SeedOnStartup";

    public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    public const string AppSettingsFileName = "appsettings.json";
    public const string AppSettingsSecretsFileName = "appsettings.secrets.json";
    public const string AppSettingsEnvironmentFileFormat = "appsettings.{0}.json";
}
