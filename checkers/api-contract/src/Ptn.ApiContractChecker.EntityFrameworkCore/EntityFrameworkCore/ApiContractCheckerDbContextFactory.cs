using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Ptn.ApiContractChecker.Constants;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: EF Core design-time komutlari (migrations add/update) icin uygulama DbContext'ini olusturur.
// sistemdeki gorevi: Migration uretimi host calismadan da appsettings + schema ayarlarini okuyarak runtime ile birebir ayni modeli kurabilsin; boylece ayri bir migration DbContext'ine gerek kalmaz.
public class ApiContractCheckerDbContextFactory : IDesignTimeDbContextFactory<ApiContractCheckerDbContext>
{
    public ApiContractCheckerDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        ApiContractCheckerEntityFrameworkCoreModule.ConfigureSchemas(configuration);

        // Runtime modulu (ApiContractCheckerEntityFrameworkCoreModule) snake_case kullaniyor; migration'lar da ayni olsun diye burada da uygulanir, yoksa design-time PascalCase ile runtime snake_case birbirini tutmaz.
        var connectionString = configuration.GetConnectionString(ApiContractCheckerDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString(ApiContractCheckerDatabaseConstants.DefaultConnectionStringName)
                               ?? "Host=localhost;Database=pintern_api_contract_checker_design";
        var builder = new DbContextOptionsBuilder<ApiContractCheckerDbContext>()
            .UseNpgsql(connectionString, postgreSqlOptions =>
                postgreSqlOptions.MigrationsHistoryTable(
                    ApiContractCheckerDatabaseConstants.MigrationsHistoryTableName,
                    ApiContractCheckerDbProperties.CheckerSchema))
            .UseSnakeCaseNamingConvention();

        return new ApiContractCheckerDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = ResolveConfigurationBasePath();
        var environment = Environment.GetEnvironmentVariable(ApiContractCheckerConfigurationKeys.AspNetCoreEnvironmentVariable);

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(ApiContractCheckerConfigurationKeys.AppSettingsFileName, optional: true)
            .AddJsonFile(ApiContractCheckerConfigurationKeys.AppSettingsSecretsFileName, optional: true)
            .AddJsonFile(string.Format(ApiContractCheckerConfigurationKeys.AppSettingsEnvironmentFileFormat, environment), optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private static string ResolveConfigurationBasePath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDirectory is not null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, ApiContractCheckerConfigurationKeys.AppSettingsFileName)))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
