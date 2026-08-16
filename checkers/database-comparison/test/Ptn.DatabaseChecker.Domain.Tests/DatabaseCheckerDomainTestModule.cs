using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerDomainModule),
    typeof(DatabaseCheckerTestBaseModule)
)]
public class DatabaseCheckerDomainTestModule : AbpModule
{

}
