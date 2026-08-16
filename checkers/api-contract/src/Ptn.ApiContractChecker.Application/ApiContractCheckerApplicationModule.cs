using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Application.BackgroundWorkers.Sources;
using Ptn.ApiContractChecker.Configuration;
using Volo.Abp;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerDomainModule),
    typeof(ApiContractCheckerApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundJobsModule),
    // Zamanlanmis izlemenin periyodik tarayicisi ABP'nin kendi worker altyapisinda calisir; Quartz/Hangfire eklenmez (ADR-0009).
    typeof(AbpBackgroundWorkersModule)
    )]
public class ApiContractCheckerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.AddMapperlyObjectMapper<ApiContractCheckerApplicationModule>();
        context.Services.AddHttpClient();

        // Izleme esikleri appsettings'ten baglanir; anlamlilik denetimi typed validator'a aittir ve uygulama
        // ayaga kalkarken calisir. Options tipi Domain.Shared'da yasar, kaydi tuketicinin kendi modulundedir.
        Configure<SpecMonitoringOptions>(configuration.GetSection(SpecMonitoringOptions.SectionName));
        context.Services.AddSingleton<IValidateOptions<SpecMonitoringOptions>, SpecMonitoringOptionsValidator>();
        context.Services.AddOptions<SpecMonitoringOptions>().ValidateOnStart();
    }

    // Periyodik tarayiciyi ABP worker yoneticisine baglar; is yapan taraf kuyruktaki job'dir.
    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        return context.AddBackgroundWorkerAsync<DueSpecDocumentCheckWorker>();
    }
}
