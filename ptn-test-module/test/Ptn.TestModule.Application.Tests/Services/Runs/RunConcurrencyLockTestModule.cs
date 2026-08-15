using Volo.Abp.DistributedLocking;
using Volo.Abp.Modularity;

namespace Ptn.TestModule.Application.Tests.Services.Runs;

// islevi: Kosum eszamanlilik testine yalniz ABP'nin yerel kilit implementasyonunu kurar.
// sistemdeki gorevi: Uygulamanin ilgisiz OpenIddict ve repository modullerini kilit davranisi kanitindan ayirir.
[DependsOn(typeof(AbpDistributedLockingAbstractionsModule))]
public class RunConcurrencyLockTestModule : AbpModule
{
}
