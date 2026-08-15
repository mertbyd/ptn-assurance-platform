using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Ajan butcesi, MCP Task ve Overlay kararlarini kalici regresyon kapisina cevirir.
// sistemdeki gorevi: Protokol tesisatinin model veya otomatik uygulama davranisina kaymasini engeller.
public class AgentProtocolManagerTests
{
    [Fact]
    public void Should_reject_tool_budget_exhaustion_with_the_existing_error_code()
    {
        var profile = new AgentProfile { AllowedToolCodes = [PtnToolCodes.Ground], MaxTurns = 2, TokenLimit = 100 };
        var exception = Should.Throw<BusinessException>(() =>
            new ToolBudgetManager().EnsureWithinBudget(profile, PtnToolCodes.Ground, 2, 10));
        exception.Code.ShouldBe("TestModule.Bridge:ToolBudgetExceeded");
    }

    [Theory]
    [InlineData(TestRunStatusCodes.Pending, false, false, McpTaskStatusCodes.Working)]
    [InlineData(TestRunStatusCodes.Running, false, false, McpTaskStatusCodes.Working)]
    [InlineData(TestRunStatusCodes.Completed, false, false, McpTaskStatusCodes.Completed)]
    [InlineData(TestRunStatusCodes.Completed, false, true, McpTaskStatusCodes.Failed)]
    [InlineData(TestRunStatusCodes.Cancelled, false, false, McpTaskStatusCodes.Cancelled)]
    [InlineData(TestRunStatusCodes.Pending, true, false, McpTaskStatusCodes.InputRequired)]
    public void Should_map_task_status_without_loss(string internalStatus, bool approval, bool infrastructureFailure, string expected)
    {
        var result = new McpTaskStatusManager().Map("task-1", internalStatus, approval, infrastructureFailure, 60000, 1000);
        result.Status.ShouldBe(expected);
        result.TtlMs.ShouldBe(60000);
        result.PollIntervalMs.ShouldBe(1000);
    }

    [Fact]
    public void Should_create_review_only_overlay_bound_to_the_finding()
    {
        var fingerprint = new string('a', 64);
        var result = new OverlayPatchManager().Suggest(fingerprint, "$.paths['/tickets']", "Declare the observed response", "{\"description\":\"observed\"}");
        result.FindingFingerprint.ShouldBe(fingerprint);
        result.Document.ShouldContain("\"overlay\": \"1.0.0\"");
        result.Applied.ShouldBeFalse();
    }
}
