using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Runs;

// islevi: Kosum custom repository ve AppService kayitlarinin gercek ABP modul grafinde cozuldugunu dogrular.
// sistemdeki gorevi: Dosyalari derlenen fakat DI kompozisyonuna baglanmayan eksik dikey dilimleri engeller.
/// <summary>Test kosumu dependency injection baglanti testleridir.</summary>
public class TestRunWiringTests : TestModuleEntityFrameworkCoreTestBase
{
    /// <summary>Run ve result custom repository arayuzlerinin provider implementasyonlarina baglandigini dogrular.</summary>
    [Fact]
    public void Should_resolve_custom_run_repositories()
    {
        ServiceProvider.GetRequiredService<ITestRunRepository>().ShouldNotBeNull();
        ServiceProvider.GetRequiredService<ITestRunResultRepository>().ShouldNotBeNull();
    }

    /// <summary>Run AppService sozlesmesinin tum Manager ve repository bagimliliklariyla cozuldugunu dogrular.</summary>
    [Fact]
    public void Should_resolve_run_app_service()
    {
        ServiceProvider.GetRequiredService<ITestRunAppService>().ShouldNotBeNull();
    }
}
