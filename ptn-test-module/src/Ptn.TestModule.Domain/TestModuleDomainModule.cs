using Microsoft.Extensions.DependencyInjection;
using Pintern.Authenticator;
using Pintern.Notifications;
using Ptn.TestModule.Managers.Bridge;
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
    // Kopru manager'larini acik ve tekil domain sahipleri olarak DI'a kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ProfilePackManager>();
        context.Services.AddTransient<EvidenceChainManager>();
    }
}
