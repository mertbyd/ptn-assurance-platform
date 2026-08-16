using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

[DependsOn(
    typeof(TestModuleDomainModule),
    typeof(TestModuleTestBaseModule)
)]
public class TestModuleDomainTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var mockPort = NSubstitute.Substitute.For<Ptn.TestModule.Interface.Bridge.IBusinessRuleSourcePort>();
        mockPort.ReadAsync(default).ReturnsForAnyArgs(System.Threading.Tasks.Task.FromResult(System.Text.Encoding.UTF8.GetBytes("mock-rules")));
        context.Services.AddSingleton(mockPort);
    }
}
