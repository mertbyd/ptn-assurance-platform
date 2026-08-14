using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator.EntityFrameworkCore;
using Pintern.Notifications.EntityFrameworkCore;
using Ptn.TestModule.Adapters;
using Ptn.TestModule.Constants;
using Ptn.TestModule.Interface.Bridge;
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
        context.Services.AddTransient<IProfilePackProvider, ProfilePackFileProvider>();

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
        });
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
