using Ptn.ApiContractChecker;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace CheckNexus.ApiContracts;

// islevi: API Contract Checker'in application, HTTP API ve EF Core katmanlarini tek ABP bagimlilik noktasinda toplar.
// sistemdeki gorevi: Composition Host veya Test modulunun tek NuGet paketi ve tek DependsOn bildirimiyle tum checker yuzeyini eklemesini saglar.
[DependsOn(
    typeof(ApiContractCheckerApplicationModule),
    typeof(ApiContractCheckerEntityFrameworkCoreModule),
    typeof(ApiContractCheckerHttpApiModule)
)]
public class ApiContractCheckerModule : AbpModule
{
}
