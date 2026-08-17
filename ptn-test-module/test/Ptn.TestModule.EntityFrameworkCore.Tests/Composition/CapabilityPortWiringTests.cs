using Ptn.TestModule.Interface.Authoring;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Interface.Shared;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Tests.Composition;

// islevi: Capability port sozlesmelerinin gercek ABP modul grafinde uygulamalarina cozuldugunu dogrular.
// sistemdeki gorevi: ABP varsayilan servis kurali (arayuz adi ile sinif adi eslesmesi) tutmayan port
// uygulamalarinin [ExposeServices] olmadan kayitsiz kalmasini engeller; eksik kayit derlemede degil
// ilk HTTP isteginde 500 olarak gorunur.
/// <summary>Capability port dependency injection baglanti testleridir.</summary>
public class CapabilityPortWiringTests : TestModuleTestBase<TestModuleIsolatedAuthTestModule>
{
    /// <summary>Yazarlik ve derleme port sozlesmelerinin cozuldugunu dogrular.</summary>
    [Fact]
    public void Should_resolve_authoring_ports()
    {
        GetRequiredService<IAuthoringSessionStore>().ShouldNotBeNull();
        GetRequiredService<IScenarioCompilationPort>().ShouldNotBeNull();
    }

    /// <summary>Bridge kaynak port sozlesmelerinin cozuldugunu dogrular.</summary>
    [Fact]
    public void Should_resolve_bridge_source_ports()
    {
        GetRequiredService<IAgentPolicySourcePort>().ShouldNotBeNull();
        GetRequiredService<IBusinessRuleSourcePort>().ShouldNotBeNull();
        GetRequiredService<IProfilePackSourcePort>().ShouldNotBeNull();
    }

    /// <summary>Kosum hattinin port sozlesmelerinin cozuldugunu dogrular.</summary>
    [Fact]
    public void Should_resolve_run_pipeline_ports()
    {
        GetRequiredService<IWorkflowRunnerPort>().ShouldNotBeNull();
        GetRequiredService<IOracleDispatchPort>().ShouldNotBeNull();
        GetRequiredService<ITestDataSandbox>().ShouldNotBeNull();
        GetRequiredService<IProcessBoundaryPort>().ShouldNotBeNull();
    }

    /// <summary>Kosum port'larinin bagli oldugu provider adapter'lerinin cozuldugunu dogrular.</summary>
    [Fact]
    public void Should_resolve_run_provider_adapters()
    {
        GetRequiredService<ITestDataSandboxConnectionFactory>().ShouldNotBeNull();
        GetRequiredService<IRunCredentialPort>().ShouldNotBeNull();
    }
}
