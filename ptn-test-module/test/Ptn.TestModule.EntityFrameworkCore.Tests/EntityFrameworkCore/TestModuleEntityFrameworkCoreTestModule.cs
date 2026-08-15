using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.Data;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace Ptn.TestModule.EntityFrameworkCore;

[DependsOn(
    typeof(TestModuleApplicationTestModule),
    typeof(TestModuleEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class TestModuleEntityFrameworkCoreTestModule : AbpModule
{
    private AbpUnitTestSqliteDatabase? _database;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        _database = new AbpUnitTestSqliteDatabase();

        /* Semayi kuran context calisma anindaki ile ayni adlandirma kuralini kullanmalidir;
         * aksi halde tablolar PascalCase kolonlarla kurulur ve sorgular snake_case arar. */
        _database.CreateTables(
            new TestModuleDbContext(
                new DbContextOptionsBuilder<TestModuleDbContext>()
                    .UseSqlite(_database.ConnectionString)
                    .UseSnakeCaseNamingConvention()
                    .Options));

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = _database.ConnectionString;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.UseSqlite();
            });

            /* Uretim modulu TestModuleDbContext'i acikca Npgsql'e baglar; genel SQLite ayari onu ezmez.
             * Context'e ozel bu ayar uretim modulunden sonra kaydedildigi icin testlerde SQLite kazanir. */
            options.Configure<TestModuleDbContext>(dbContext =>
            {
                dbContext.DbContextOptions.UseSqlite(_database.ConnectionString);
                dbContext.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });
    }

    // Butun modullerin kaydi bittikten sonra calisir; katkici tip listesi bu noktada tamamlanmistir.
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        RemoveHostOwnedDataSeedContributors();
    }

    /* Test base'i uygulama baslarken IDataSeeder'i tumuyle kosturur ve ABP katkicilari DI'dan degil
     * AbpDataSeedOptions.Contributors tip listesinden cozer. Authenticator'in OpenIddict ve kimlik
     * katkicilari gercek auth yapilandirmasi bekler; modul testinde anlamlari yoktur. Yalniz Test
     * Module'un kendi seed'i birakilir, testler onu zaten acikca tetikler. */
    private void RemoveHostOwnedDataSeedContributors()
    {
        var moduleAssembly = typeof(TestModuleLookupDataSeedContributor).Assembly;

        Configure<AbpDataSeedOptions>(options =>
        {
            var hostOwnedContributors = options.Contributors
                .Where(contributor => contributor.Assembly != moduleAssembly)
                .ToList();

            foreach (var contributor in hostOwnedContributors)
            {
                options.Contributors.Remove(contributor);
            }
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _database?.Dispose();
    }

}
