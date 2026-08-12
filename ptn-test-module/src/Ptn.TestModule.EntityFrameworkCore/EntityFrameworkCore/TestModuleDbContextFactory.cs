using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Ptn.TestModule.Constants;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: EF Core design-time komutlari (migrations add/update) icin uygulama DbContext'ini olusturur.
// sistemdeki gorevi: Migration uretimi host calismadan da appsettings + sema ayarlarini okuyarak runtime
// ile birebir ayni modeli kurar; boylece ikinci bir migration DbContext'ine gerek kalmaz (RULE-0002).
public class TestModuleDbContextFactory : IDesignTimeDbContextFactory<TestModuleDbContext>
{
    public TestModuleDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        TestModuleEntityFrameworkCoreModule.ConfigureSchemas(configuration);

        // Runtime modulu snake_case kullanir; migration'lar da ayni olsun diye burada da uygulanir,
        // yoksa design-time PascalCase ile runtime snake_case birbirini tutmaz.
        var connectionString = configuration.GetConnectionString(TestModuleDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString(TestModuleDatabaseConstants.DefaultConnectionStringName)
                               ?? "Host=localhost;Database=ptn_test_module_design";

        var builder = new DbContextOptionsBuilder<TestModuleDbContext>()
            .UseNpgsql(connectionString, postgreSqlOptions =>
                postgreSqlOptions.MigrationsHistoryTable(
                    TestModuleDatabaseConstants.MigrationsHistoryTableName,
                    TestModuleDbProperties.CatalogSchema))
            .UseSnakeCaseNamingConvention();

        return new TestModuleDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = ResolveConfigurationBasePath();
        var environment = Environment.GetEnvironmentVariable(TestModuleConfigurationKeys.AspNetCoreEnvironmentVariable);

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(TestModuleConfigurationKeys.AppSettingsFileName, optional: true)
            .AddJsonFile(TestModuleConfigurationKeys.AppSettingsSecretsFileName, optional: true)
            .AddJsonFile(string.Format(TestModuleConfigurationKeys.AppSettingsEnvironmentFileFormat, environment), optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private static string ResolveConfigurationBasePath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDirectory is not null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, TestModuleConfigurationKeys.AppSettingsFileName)))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
