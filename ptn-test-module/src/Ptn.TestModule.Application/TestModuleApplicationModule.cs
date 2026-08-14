using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator;
using Pintern.Notifications;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
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
    typeof(AuthenticatorApplicationModule),
    typeof(NotificationsApplicationModule)
)]
public class TestModuleApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<TestModuleApplicationModule>();
        context.Services.AddTransient<IApiOraclePort, ApiOracleAppService>();
        context.Services.AddTransient<IDatabaseOraclePort, DatabaseOracleAppService>();
        context.Services.AddTransient<IFailureDiagnosisPort, FailureDiagnosisAppService>();
        context.Services.AddTransient<ISchemaKnowledgePort, SchemaKnowledgeAppService>();
    }
}
