using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.FluentValidation;
using Volo.Abp.Modularity;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpFluentValidationModule)
)]
public class DatabaseCheckerApplicationContractsModule : AbpModule
{

}
