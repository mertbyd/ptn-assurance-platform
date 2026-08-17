using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator.EntityFrameworkCore;
using Pintern.Notifications.EntityFrameworkCore;
using Ptn.TestModule.Constants;
using Ptn.TestModule.Data;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.EntityFrameworkCore.Repository.Lookups;
using Ptn.TestModule.EntityFrameworkCore.Repository.Runs;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: Test Module kalici veri erisimini, sema sahipligini ve provider secimini kurar.
// sistemdeki gorevi: Yalniz kendi test_lookup/test_catalog/test_run tablolarinin migration sahibidir;
// Auth ve Notification tablolarini ikinci kez uretmez (RULE-0002, ADR-0016 §A).
[DependsOn(
    typeof(TestModuleDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AuthenticatorEntityFrameworkCoreModule),
    typeof(NotificationsEntityFrameworkCoreModule)
)]
public class TestModuleEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureSchemas(context.Services.GetConfiguration());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<TestModuleDbContext>(dbContext =>
            {
                dbContext.UseNpgsql(postgreSqlOptions =>
                    postgreSqlOptions.MigrationsHistoryTable(
                        TestModuleDatabaseConstants.MigrationsHistoryTableName,
                        TestModuleDbProperties.CatalogSchema));
                dbContext.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });

        context.Services.AddAbpDbContext<TestModuleDbContext>(options =>
        {
            options.AddDefaultRepositories<ITestModuleDbContext>(includeAllEntities: true);

            // Lookup depolari Foundation tabanindan gelir; varsayilan depo kaydi bu arayuzleri cozmez.
            options.AddRepository<TestRunStatus, EfCoreTestRunStatusRepository>();
            options.AddRepository<TestOutcomeStatus, EfCoreTestOutcomeStatusRepository>();
            options.AddRepository<TestFailureCategory, EfCoreTestFailureCategoryRepository>();
            options.AddRepository<TestTriggerKind, EfCoreTestTriggerKindRepository>();
            options.AddRepository<TestScenarioState, EfCoreTestScenarioStateRepository>();
            options.AddRepository<TestRun, EfCoreTestRunRepository>();
            options.AddRepository<TestRunResult, EfCoreTestRunResultRepository>();
        });
    }

    // Migration ve seed yalniz bu modulun sahip oldugu context ve lookup contributor'u icin kosar.
    public override async Task OnPreApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var autoMigrate = configuration.GetValue<bool>(TestModuleConfigurationKeys.DatabaseAutoMigrate);
        var seedOnStartup = configuration.GetValue<bool>(TestModuleConfigurationKeys.DatabaseSeedOnStartup);

        EnsureValidDatabaseInitializationOptions(autoMigrate, seedOnStartup);
        if (!autoMigrate)
        {
            return;
        }

        using var scope = context.ServiceProvider.CreateScope();
        await MigrateAndSeedAsync(scope.ServiceProvider, seedOnStartup);
    }

    // Seed'in migration tamamlanmadan kosulmasini engeller.
    private static void EnsureValidDatabaseInitializationOptions(bool autoMigrate, bool seedOnStartup)
    {
        if (seedOnStartup && !autoMigrate)
        {
            throw new InvalidOperationException(
                $"{TestModuleConfigurationKeys.DatabaseSeedOnStartup} requires " +
                $"{TestModuleConfigurationKeys.DatabaseAutoMigrate}.");
        }
    }

    // Sahip olunan semayi kurar ve istenirse yalniz Test Module lookup verisi ile ajan izinlerini uretir.
    private static async Task MigrateAndSeedAsync(IServiceProvider serviceProvider, bool seedOnStartup)
    {
        await serviceProvider
            .GetRequiredService<TestModuleDbContext>()
            .Database
            .MigrateAsync();

        if (!seedOnStartup)
        {
            return;
        }

        await serviceProvider
            .GetRequiredService<TestModuleLookupDataSeedContributor>()
            .SeedAsync(new DataSeedContext());

        /* Ajan izinleri paylasilan abp.AbpPermissionGrants tablosuna yazilir; tabloyu Authenticator
         * migration'i kurar, grant'i ise izinleri tanimlayan bu host verir (RULE-0002, ADR-0013). */
        await serviceProvider
            .GetRequiredService<AgentClientPermissionDataSeedContributor>()
            .SeedAsync(new DataSeedContext());
    }

    // Sema adlari ortam bazli ezilebilir; ezme yoksa Domain.Shared varsayilanlari korunur.
    public static void ConfigureSchemas(IConfiguration configuration)
    {
        var schemas = configuration.GetSection(TestModuleConfigurationKeys.EntityFrameworkCoreSchemasSection);
        if (!schemas.Exists())
        {
            return;
        }

        TestModuleDbProperties.LookupSchema =
            schemas[TestModuleConfigurationKeys.LookupSchema] ?? TestModuleDbProperties.LookupSchema;
        TestModuleDbProperties.CatalogSchema =
            schemas[TestModuleConfigurationKeys.CatalogSchema] ?? TestModuleDbProperties.CatalogSchema;
        TestModuleDbProperties.RunSchema =
            schemas[TestModuleConfigurationKeys.RunSchema] ?? TestModuleDbProperties.RunSchema;
    }
}
