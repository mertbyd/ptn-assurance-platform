using CheckNexus.ApiContracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.EntityFrameworkCore.Sources;
using Ptn.ApiContractChecker.Interface.Secrets;
using Ptn.ApiContractChecker.MultiTenancy;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: EF integration testlerinin servis ikamelerini ve yalitilmis SQLite veritabanini yapilandirir.
// sistemdeki gorevi: Uretim DbContext davranisini dis Vault ve HTTP bagimliligi olmadan deterministik olarak dogrulatir.
[DependsOn(
    typeof(ApiContractCheckerApplicationTestModule),
    typeof(ApiContractCheckerModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class ApiContractCheckerEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection? _databaseKeeper;

    // Test servislerini ve her ABP test uygulamasina ozel SQLite veritabanini kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var commandCaptureInterceptor = new SqlCommandCaptureInterceptor();
        context.Services.AddSingleton(commandCaptureInterceptor);

        // EF integration testleri gercek Vault ve dis HTTP yerine ayni uretim kontratlarinin bellek implementasyonlarini kullanir.
        context.Services.Replace(ServiceDescriptor.Singleton<ISecretProvider, InMemorySpecSourceSecretProvider>());
        context.Services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory, SpecSourceHttpClientFactory>());

        // Kuyruk yazimi gercek ABP store'una gider, worker calismaz: testler kuyrugu deterministik okur ve job'i kendi tetikler.
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        // Boyut guard'i test govdelerinde bellek sisirmeden sinir ve max + 1 okuma davranisini kanitlar.
        Configure<SpecFetchOptions>(options =>
        {
            options.MaxContentSizeBytes = 64;
        });

        // Tenancy davranisi hostlarla ayni tek bayraktan okunur (ikinci bir yerde kodlanmaz).
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        var sqliteConnectionString = CreateDatabaseAndGetConnectionString();

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings[ApiContractCheckerDbProperties.ConnectionStringName] =
                sqliteConnectionString;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<ApiContractCheckerDbContext>(ctx =>
            {
                ctx.DbContextOptions.UseSqlite(sqliteConnectionString);
                ctx.DbContextOptions.UseSnakeCaseNamingConvention();
                ctx.DbContextOptions.AddInterceptors(commandCaptureInterceptor);
            });
        });
    }

    // Test uygulamasi boyunca yasayan veritabanini kurup DbContext'lerin ayri baglantilarla erisecegi adresi dondurur.
    private string CreateDatabaseAndGetConnectionString()
    {
        var connectionString = $"Data Source=acc-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False";
        _databaseKeeper = new SqliteConnection(connectionString);
        _databaseKeeper.Open();

        new ApiContractCheckerDbContext(
            new DbContextOptionsBuilder<ApiContractCheckerDbContext>()
                .UseSqlite(_databaseKeeper)
                .UseSnakeCaseNamingConvention()
                .Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connectionString;
    }

    // Test uygulamasi kapanirken yalniz ona ait bellek veritabanini serbest birakir.
    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _databaseKeeper?.Dispose();
    }
}
