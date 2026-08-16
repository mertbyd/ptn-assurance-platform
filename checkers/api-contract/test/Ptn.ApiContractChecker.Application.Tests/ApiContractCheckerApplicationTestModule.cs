using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerApplicationModule),
    typeof(ApiContractCheckerDomainTestModule)
    )]
public class ApiContractCheckerApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Periyodik worker test uygulamasinda kendiliginden tiklemez: testler tarama tikini
        // kendi tetikler ve zamanlamaya bagli kuyruk yan etkisi olusmaz
        // (kuyruk yurutmesi icin ayni karar AbpBackgroundJobOptions'ta verilmistir).
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });
    }
}
