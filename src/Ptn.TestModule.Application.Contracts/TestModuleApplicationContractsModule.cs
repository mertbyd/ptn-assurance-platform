using Pintern.Authenticator;
using Pintern.Notifications;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.FluentValidation;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

// islevi: Test Module DTO, AppService sozlesmesi ve permission tanimlarini ABP'ye kaydeder.
// sistemdeki gorevi: Her public girdi DTO'sunun FluentValidation validator'uyla dogrulanmasini saglar.
[DependsOn(
    typeof(TestModuleDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpFluentValidationModule),
    typeof(AuthenticatorApplicationContractsModule),
    typeof(NotificationsApplicationContractsModule)
)]
public class TestModuleApplicationContractsModule : AbpModule
{
}
