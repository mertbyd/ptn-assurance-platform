using Volo.Abp.Modularity;

namespace Ptn.TestModule;

[DependsOn(
    typeof(TestModuleDomainModule),
    typeof(TestModuleTestBaseModule)
)]
public class TestModuleDomainTestModule : AbpModule
{

}
