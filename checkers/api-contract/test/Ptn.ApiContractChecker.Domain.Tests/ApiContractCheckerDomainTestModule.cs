using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerDomainModule),
    typeof(ApiContractCheckerTestBaseModule)
)]
public class ApiContractCheckerDomainTestModule : AbpModule
{

}
