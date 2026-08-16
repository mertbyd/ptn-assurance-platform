using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class DatabaseCheckerHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(DatabaseCheckerApplicationContractsModule).Assembly,
            DatabaseCheckerRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<DatabaseCheckerHttpApiClientModule>();
        });

    }
}
