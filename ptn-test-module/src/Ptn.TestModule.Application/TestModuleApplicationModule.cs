using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator;
using Pintern.Notifications;
using Ptn.DatabaseChecker;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
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
    typeof(AbpBlobStoringModule),
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
}
