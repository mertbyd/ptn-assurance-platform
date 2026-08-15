using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TestModuleHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelModule)
    )]
public class TestModuleConsoleApiClientModule : AbpModule
{

}
