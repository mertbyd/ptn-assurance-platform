using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerApplicationModule),
    typeof(DatabaseCheckerDomainTestModule)
    )]
public class DatabaseCheckerApplicationTestModule : AbpModule
{

}
