using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class ApiContractCheckerHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(ApiContractCheckerApplicationContractsModule).Assembly,
            ApiContractCheckerRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ApiContractCheckerHttpApiClientModule>();
        });

    }
}
