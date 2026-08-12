using Pintern.Authenticator;
using Pintern.Notifications;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

// islevi: Test Module domain katmaninin ABP modul bagimliliklarini tanimlar.
// sistemdeki gorevi: Senaryo, kosum ve is bilgisi manager'larinin kompozisyon kokudur.
[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(TestModuleDomainSharedModule),
    typeof(AuthenticatorDomainModule),
    typeof(NotificationsDomainModule)
)]
public class TestModuleDomainModule : AbpModule
{
}
