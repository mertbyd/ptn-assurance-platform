using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Ptn.DatabaseChecker.Constants;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

// islevi: EF Core design-time komutlari (migrations add/update) icin uygulama DbContext'ini olusturur.
// sistemdeki gorevi: Migration uretimi host calismadan da appsettings + schema ayarlarini okuyarak runtime ile birebir ayni modeli kurabilsin; boylece ayri bir migration DbContext'ine gerek kalmaz.
public class DatabaseCheckerDbContextFactory : IDesignTimeDbContextFactory<DatabaseCheckerDbContext>
{
    public DatabaseCheckerDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        DatabaseCheckerEntityFrameworkCoreModule.ConfigureSchemas(configuration);

        // Runtime modulu (DatabaseCheckerEntityFrameworkCoreModule) snake_case kullaniyor; migration'lar da ayni olsun diye burada da uygulanir, yoksa design-time PascalCase ile runtime snake_case birbirini tutmaz.
        var connectionString = configuration.GetConnectionString(DatabaseCheckerDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString(DatabaseCheckerDatabaseConstants.DefaultConnectionStringName)
                               ?? "Host=localhost;Database=pintern_database_checker_design";
        var builder = new DbContextOptionsBuilder<DatabaseCheckerDbContext>()
            .UseNpgsql(connectionString, postgreSqlOptions =>
                postgreSqlOptions.MigrationsHistoryTable(
                    DatabaseCheckerDatabaseConstants.MigrationsHistoryTableName,
                    DatabaseCheckerDbProperties.LookupsSchema))
            .UseSnakeCaseNamingConvention();

        return new DatabaseCheckerDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = ResolveConfigurationBasePath();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.secrets.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private static string ResolveConfigurationBasePath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDirectory is not null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "appsettings.json")))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
