using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.FluentValidation;
using Volo.Abp.Modularity;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpFluentValidationModule)
)]
public class ApiContractCheckerApplicationContractsModule : AbpModule
{

}
