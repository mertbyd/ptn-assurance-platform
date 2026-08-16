using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerDomainModule),
    typeof(DatabaseCheckerApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundJobsModule)
    )]
public class DatabaseCheckerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<DatabaseCheckerApplicationModule>();
        context.Services.AddHttpClient();
    }
}
