using CheckNexus.DatabaseComparison;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Ptn.DatabaseChecker.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

[DependsOn(
    typeof(DatabaseCheckerApplicationTestModule),
    typeof(DatabaseCheckerModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class DatabaseCheckerEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Tenancy davranisi hostlarla ayni tek bayraktan okunur (ikinci bir yerde kodlanmaz).
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings[DatabaseCheckerDbProperties.ConnectionStringName] =
                sqliteConnection.ConnectionString;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<DatabaseCheckerDbContext>(ctx =>
            {
                ctx.UseSqlite(sqliteConnection);
                ctx.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new AbpUnitTestSqliteConnection("Data Source=:memory:");
        connection.Open();

        new DatabaseCheckerDbContext(
            new DbContextOptionsBuilder<DatabaseCheckerDbContext>()
                .UseSqlite(connection)
                .UseSnakeCaseNamingConvention()
                .Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
