using Microsoft.Extensions.DependencyInjection;
using Ptn.ApiContractChecker.Managers.Lookups;
using Volo.Abp.Domain;
using Volo.Abp.Timing;
using Volo.Abp.Modularity;
namespace Ptn.ApiContractChecker;
[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ApiContractCheckerDomainSharedModule)
)]
public class ApiContractCheckerDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Npgsql "timestamp with time zone" kolonuna yalniz Kind=Utc DateTime yazar; ABP'nin varsayilan
        // saati Kind=Unspecified uretir ve her Clock.Now yazimi InvalidCastException ile duser
        // (run.Start, MarkSeen, snapshot acilisi). Kind burada sabitlenince hem Clock.Now hem
        // CreationTime/LastModificationTime denetim alanlari UTC olarak normalize edilir.
        Configure<AbpClockOptions>(options => options.Kind = DateTimeKind.Utc);

        // Acik-generic manager konvansiyonel DI ile toplanamaz (ABP kapali tipleri tarar); IBaseRepository<>/ISpecFormatComponentResolver<> ile ayni kalip: tek acik-generic kayit tum LookupManager<TEntity> cozumlerine baglanir.
        context.Services.AddTransient(typeof(LookupManager<>), typeof(LookupManager<>));
    }
}
