using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Modularity;
using ApiSecretProvider = Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider;
using DatabaseSecretProvider = Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider;

namespace CheckNexus.Vault;

// islevi: Merkezî Vault adapter'ini executable composition hostun ABP module graph'ina ekler.
// sistemdeki gorevi: Tek provider instance'ini iki checker'in farkli secret portlarina explicit kaydeder ve typed configuration'i fail-fast dogrular.
public sealed class CheckNexusVaultModule : AbpModule
{
    // Vault options, HTTP client ve iki checker portunun tek implementation kaydini kurar.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<VaultOptions>(configuration.GetSection(VaultOptions.SectionName));
        context.Services.AddSingleton<IValidateOptions<VaultOptions>, VaultOptionsValidator>();
        context.Services.AddOptions<VaultOptions>().ValidateOnStart();
        context.Services.AddHttpClient(VaultConstants.HttpClientName);

        context.Services.AddSingleton<VaultSecretProvider>();
        context.Services.AddSingleton<ApiSecretProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<VaultSecretProvider>());
        context.Services.AddSingleton<DatabaseSecretProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<VaultSecretProvider>());
    }
}
