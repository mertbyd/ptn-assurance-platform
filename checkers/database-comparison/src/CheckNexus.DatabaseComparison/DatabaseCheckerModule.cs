using Ptn.DatabaseChecker;
using Ptn.DatabaseChecker.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace CheckNexus.DatabaseComparison;

// islevi: Database Checker'in application, HTTP API ve EF Core katmanlarini tek ABP bagimlilik noktasinda toplar.
// sistemdeki gorevi: Composition Host veya Test modulunun tek NuGet paketi ve tek DependsOn bildirimiyle tum checker yuzeyini eklemesini saglar.
[DependsOn(
    typeof(DatabaseCheckerApplicationModule),
    typeof(DatabaseCheckerEntityFrameworkCoreModule),
    typeof(DatabaseCheckerHttpApiModule)
)]
public class DatabaseCheckerModule : AbpModule
{
}
