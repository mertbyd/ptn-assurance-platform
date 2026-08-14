using Volo.Abp.Modularity;

namespace Ptn.TestModule;

[DependsOn(
    typeof(TestModuleApplicationModule),
    typeof(TestModuleDomainTestModule)
    )]
public class TestModuleApplicationTestModule : AbpModule
{

}
