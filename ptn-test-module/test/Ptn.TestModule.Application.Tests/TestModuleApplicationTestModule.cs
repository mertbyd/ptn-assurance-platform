using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Ptn.TestModule;

[DependsOn(
    typeof(TestModuleApplicationModule),
    typeof(TestModuleDomainTestModule)
    )]
public class TestModuleApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton(Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>());
    }
}
