using CheckNexus.Vault;
using Volo.Abp.Modularity;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Vault composition testinin yalniz gerekli ABP module graph'i ile calismasini saglar.
// sistemdeki gorevi: Secret degeri gerektirmeyen AgentProxy ayariyla gercek Vault DI kayitlarini acar.
[DependsOn(typeof(CheckNexusVaultModule))]
public class VaultCompositionTestModule : AbpModule
{
    // Vault options validation'ini gercek secret kullanmadan baslatir.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<VaultOptions>(options =>
        {
            options.Address = "http://127.0.0.1:8200";
            options.AuthenticationMode = VaultAuthenticationMode.AgentProxy;
        });
    }
}
