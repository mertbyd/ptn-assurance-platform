using System;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities;
using Ptn.DatabaseChecker.Interface;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Repository;
using Ptn.DatabaseChecker.Search;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

[DependsOn(
    typeof(DatabaseCheckerDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
// islevi: DatabaseChecker kalici veri erisimi, provider secimi ve EF tabanli entegrasyonlarini kurar.
// sistemdeki gorevi: Tuketici hosta yalniz modul tablolari, repository, veri-motoru adapterleri ve arama altyapisini ekler.
public class DatabaseCheckerEntityFrameworkCoreModule : AbpModule
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

        // Elasticsearch: typed options + tek singleton client. ES client'a dokunan repository tabani yalnizca EFCore katmanindadir.
        Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));
        context.Services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            if (!Uri.TryCreate(options.Url, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("Elasticsearch Url configuration is invalid.");
            }

            var timeoutSeconds = Math.Max(1, options.RequestTimeoutSeconds);
            return new ElasticsearchClient(new ElasticsearchClientSettings(uri)
                .RequestTimeout(TimeSpan.FromSeconds(timeoutSeconds)));
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<DatabaseCheckerDbContext>(ctx =>
            {
                ctx.UseNpgsql(postgreSqlOptions =>
                    postgreSqlOptions.MigrationsHistoryTable(
                        DatabaseCheckerDatabaseConstants.MigrationsHistoryTableName,
                        DatabaseCheckerDbProperties.LookupsSchema));
                ctx.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });

        context.Services.AddAbpDbContext<DatabaseCheckerDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Ozel repository tabani IBaseRepository<> -> BaseRepository<> tek acik-generic kayitla tum entity'lere baglanir
        // (golden rule 1/2: her entity icin ayri kayit yerine tek generic kayit, ABP EfCoreRepository altyapisi korunur).
        context.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Motor-ozel bilesen resolver'i (schema reader / connection tester) tek acik-generic kayitla baglanir; IBaseRepository ile ayni kalip
        // (ABP acik-generic servisi convention ile toplayamaz; bilesenlerin kendisi ITransientDependency ile otomatik yakalanir).
        context.Services.AddTransient(typeof(IEngineComponentResolver<>), typeof(EngineComponentResolver<>));
    }

    public static void ConfigureSchemas(IConfiguration configuration)
    {
        // ABP, OpenIddict ve Piton.Emailing sema anahtarlari Domain.Shared sozlesmesinde korunur.
        // Bu modul yalniz sahip oldugu checker/operator/email entegrasyon semalarini ayarlar;
        // ortak Auth ve Notification modulleri kendi global DbProperties degerlerini ve migration'larini uygular.
        var schemas = configuration.GetSection(DatabaseCheckerConfigurationKeys.EntityFrameworkCoreSchemasSection);
        if (!schemas.Exists())
        {
            return;
        }

        DatabaseCheckerDbProperties.LookupsSchema =
            schemas[DatabaseCheckerConfigurationKeys.LookupsSchema] ?? DatabaseCheckerDbProperties.LookupsSchema;
        DatabaseCheckerDbProperties.ConnectionsSchema =
            schemas[DatabaseCheckerConfigurationKeys.ConnectionsSchema] ?? DatabaseCheckerDbProperties.ConnectionsSchema;
        DatabaseCheckerDbProperties.DefinitionsSchema =
            schemas[DatabaseCheckerConfigurationKeys.DefinitionsSchema] ?? DatabaseCheckerDbProperties.DefinitionsSchema;
        DatabaseCheckerDbProperties.RunsSchema =
            schemas[DatabaseCheckerConfigurationKeys.RunsSchema] ?? DatabaseCheckerDbProperties.RunsSchema;
        DatabaseCheckerDbProperties.OperatorsSchema =
            schemas[DatabaseCheckerConfigurationKeys.OperatorsSchema] ?? DatabaseCheckerDbProperties.OperatorsSchema;
        DatabaseCheckerDbProperties.ComparisonSchema =
            schemas[DatabaseCheckerConfigurationKeys.ComparisonSchema] ?? DatabaseCheckerDbProperties.ComparisonSchema;
        DatabaseCheckerDbProperties.EmailSchema =
            schemas[DatabaseCheckerConfigurationKeys.EmailSchema] ?? DatabaseCheckerDbProperties.EmailSchema;
    }
}
