using System;
using System.Collections.Generic;
using Ptn.TestModule.Constants.Shared;
using Ptn.TestModule.Managers.Shared;
using Ptn.TestModule.Models.Shared;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Domain.Tests.Shared;

// islevi: Surec planinin validasyon, workspace, token ve ortam descriptor kurallarini dogrular.
// sistemdeki gorevi: Process I/O'sundan ayrilan saf Manager kararlarini hizli unit testlerle sabitler.
public class ProcessPlanManagerTests
{
    // Null plan descriptor olusturulmadan reddedilmelidir.
    [Fact]
    public void Should_reject_a_null_plan()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessPlanManager().CreateDescriptor(null!, "temp"));
    }

    // Ayni plan her cagri icin farkli ve izole workspace koku uretmelidir.
    [Fact]
    public void Should_create_a_unique_workspace_for_each_descriptor()
    {
        var manager = new ProcessPlanManager();
        var plan = CreatePlan();

        var first = manager.CreateDescriptor(plan, "temp");
        var second = manager.CreateDescriptor(plan, "temp");

        first.Workspace.WorkspaceRoot.ShouldNotBe(second.Workspace.WorkspaceRoot);
    }

    // Workspace token'i cozulmeli ve ortam sozlugu descriptor'a aynen tasinmalidir.
    [Fact]
    public void Should_resolve_workspace_tokens_and_copy_environment_variables()
    {
        var descriptor = new ProcessPlanManager().CreateDescriptor(CreatePlan(), "temp");

        descriptor.Arguments[0].ShouldContain(descriptor.Workspace.WorkspaceRoot);
        descriptor.Arguments[0].ShouldNotContain(ProcessBoundaryConsts.WorkspaceToken);
        descriptor.EnvironmentVariables["TOKEN"].ShouldBe("secret");
    }

    // Token ve ortam tasiyan kararli test plani kurar.
    private static ProcessExecutionPlan CreatePlan()
    {
        return new ProcessExecutionPlan
        {
            Executable = "runner",
            Arguments = [$"--workspace={ProcessBoundaryConsts.WorkspaceToken}"],
            EnvironmentVariables = new Dictionary<string, string> { ["TOKEN"] = "secret" },
            WorkspaceName = "unit",
            InputFiles = [new ProcessInputFile { RelativePath = "input/test.yaml", Content = "content" }],
            OutputFilePaths = ["output/result.json"],
            TimeoutMs = 1000,
            StartFailureErrorCode = "start",
            TimeoutErrorCode = "timeout"
        };
    }
}
