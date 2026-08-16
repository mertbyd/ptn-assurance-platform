using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Interface;
using Ptn.ApiContractChecker.Interface.Formats;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Formats;
using Ptn.ApiContractChecker.Repository;
using Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

[DependsOn(
    typeof(ApiContractCheckerDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
// islevi: ApiContractChecker kalici veri erisimi, provider secimi ve EF tabanli entegrasyonlarini kurar.
// sistemdeki gorevi: Tuketici hosta yalniz modul tablolarini, repository'leri ve secret portundan bagimsiz HTTP altyapisini ekler.
public class ApiContractCheckerEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureSchemas(context.Services.GetConfiguration());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // IPassivable global filtre: varsayilan olarak aktif, IsActive == false kayitlari tum sorgulardan haric tutar.
        // Gerektiginde IDataFilter<IPassivable>.Disable() ile gecici olarak devre disi birakilabilir.
        Configure<AbpDataFilterOptions>(options =>
        {
            options.DefaultStates[typeof(IPassivable)] = new DataFilterState(isEnabled: true);
        });

        // Cekim esikleri appsettings'ten baglanir; anlamlilik denetimi typed validator'a aittir ve uygulama ayaga kalkarken calisir.
        Configure<SpecFetchOptions>(configuration.GetSection(SpecFetchOptions.SectionName));
        context.Services.AddSingleton<IValidateOptions<SpecFetchOptions>, SpecFetchOptionsValidator>();
        context.Services.AddOptions<SpecFetchOptions>().ValidateOnStart();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<ApiContractCheckerDbContext>(ctx =>
            {
                ctx.UseNpgsql(postgreSqlOptions =>
                    postgreSqlOptions.MigrationsHistoryTable(
                        ApiContractCheckerDatabaseConstants.MigrationsHistoryTableName,
                        ApiContractCheckerDbProperties.CheckerSchema));
                ctx.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });

        context.Services.AddAbpDbContext<ApiContractCheckerDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Ozel repository tabani IBaseRepository<> -> BaseRepository<> tek acik-generic kayitla tum entity'lere baglanir
        // (golden rule 1/2: her entity icin ayri kayit yerine tek generic kayit, ABP EfCoreRepository altyapisi korunur).
        context.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Format-ozel bilesen resolver.i (spec okuyucu gibi bilesenler) tek acik-generic kayitla baglanir; IBaseRepository ile ayni kalip
        // (ABP acik-generic servisi convention ile toplayamaz; bilesenlerin kendisi ITransientDependency ile otomatik yakalanir).
        context.Services.AddTransient(typeof(ISpecFormatComponentResolver<>), typeof(SpecFormatComponentResolver<>));

        // Fetch ve reachability ayni isimli tasimayi kullanir; resilience handler bu tek kayda yalniz bir kez eklenir.
        context.Services
            .AddHttpClient(SpecSourceConsts.HttpClientName)
            .AddTypedClient<ISpecFetcherClient, SpecFetcherClient>()
            .AddStandardResilienceHandler();

        context.Services
            .AddHttpClient(ProbeKindCodes.HttpClientName)
            .AddStandardResilienceHandler();
    }

    public static void ConfigureSchemas(IConfiguration configuration)
    {
        // ABP, OpenIddict ve Piton.Emailing sema anahtarlari Domain.Shared sozlesmesinde korunur.
        // Bu modul yalniz sahip oldugu checker/operator/email entegrasyon semalarini ayarlar;
        // ortak Auth ve Notification modulleri kendi global DbProperties degerlerini ve migration'larini uygular.
        var schemas = configuration.GetSection(ApiContractCheckerConfigurationKeys.EntityFrameworkCoreSchemasSection);
        if (!schemas.Exists())
        {
            return;
        }

        ApiContractCheckerDbProperties.CheckerSchema =
            schemas[ApiContractCheckerConfigurationKeys.CheckerSchema] ?? ApiContractCheckerDbProperties.CheckerSchema;
        ApiContractCheckerDbProperties.OperatorsSchema =
            schemas[ApiContractCheckerConfigurationKeys.OperatorsSchema] ?? ApiContractCheckerDbProperties.OperatorsSchema;
        ApiContractCheckerDbProperties.EmailSchema =
            schemas[ApiContractCheckerConfigurationKeys.EmailSchema] ?? ApiContractCheckerDbProperties.EmailSchema;
    }
}
