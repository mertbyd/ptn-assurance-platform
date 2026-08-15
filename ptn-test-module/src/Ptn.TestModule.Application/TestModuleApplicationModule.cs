using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator;
using Pintern.Notifications;
using Ptn.DatabaseChecker;
using Ptn.TestModule.BackgroundWorkers.Catalog;
using Ptn.TestModule.BackgroundWorkers.Runs;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

// islevi: Test Module use-case orkestrasyonunu, Mapperly eslemelerini ve arka plan is altyapisini kurar.
// sistemdeki gorevi: Kosum orkestrasyonunun ABP BackgroundJobs uzerinde calismasinin kompozisyon kokudur.
[DependsOn(
    typeof(TestModuleDomainModule),
    typeof(TestModuleApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundJobsModule),
    // Periyodik tarayicilar ABP'nin kendi worker altyapisinda calisir; Quartz/Hangfire eklenmez (ADR-0009).
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpDistributedLockingAbstractionsModule),
    typeof(AuthenticatorApplicationModule),
    typeof(NotificationsApplicationModule),
    typeof(DatabaseCheckerApplicationContractsModule)
)]
public class TestModuleApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<TestModuleApplicationModule>();
    }

    // Periyodik tarayicilari ABP worker yoneticisine baglar; is kararlarini Manager'lar verir.
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<ExpiredQuarantineSweepWorker>();
        await context.AddBackgroundWorkerAsync<ScenarioHealthRefreshWorker>();
    }
}
