using Microsoft.Extensions.DependencyInjection;
using Ptn.DatabaseChecker.Managers.Lookups;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
namespace Ptn.DatabaseChecker;
[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(DatabaseCheckerDomainSharedModule)
)]
public class DatabaseCheckerDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Acik-generic manager konvansiyonel DI ile toplanamaz (ABP kapali tipleri tarar); IBaseRepository<>/IEngineComponentResolver<> ile ayni kalip: tek acik-generic kayit tum LookupManager<TEntity> cozumlerine baglanir.
        context.Services.AddTransient(typeof(LookupManager<>), typeof(LookupManager<>));
    }
}
