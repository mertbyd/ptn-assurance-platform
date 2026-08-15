using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Services.Bridge;
using Ptn.TestModule.Services.Runs;

namespace Ptn.TestModule.Mcp;

// islevi: Mevcut Application.Contracts use-case'lerini MCP tool protokolune baglar.
// sistemdeki gorevi: Model secmeden ve checker paketlerine MCP tipi sizdirmadan composition-host yuzeyini kurar.
[McpServerToolType]
public sealed class PtnMcpTools
{
    [McpServerTool(Name = PtnToolCodes.Ground), Description("Ground an authoring operation in deterministic checker evidence.")]
    public static Task<GroundResultDto> Ground(IPtnBridgeAppService service, GroundRequestDto input) => service.GroundAsync(input);

    [McpServerTool(Name = PtnToolCodes.Knowledge), Description("Read deterministic profile knowledge and coverage.")]
    public static Task<KnowledgeResultDto> Knowledge(IPtnBridgeAppService service, KnowledgeRequestDto input) => service.GetKnowledgeAsync(input);

    [McpServerTool(Name = PtnToolCodes.Validate), Description("Validate derivability and publication evidence.")]
    public static Task<ValidateResultDto> Validate(IPtnBridgeAppService service, ValidateRequestDto input) => service.ValidateAsync(input);

    [McpServerTool(Name = PtnToolCodes.Explain), Description("Explain the deterministic evidence chain.")]
    public static Task<ExplainResultDto> Explain(IPtnBridgeAppService service, ExplainRequestDto input) => service.ExplainAsync(input);

    [McpServerTool(Name = PtnToolCodes.Run), Description("Queue a published scenario run without invoking a model.")]
    public static Task<TestRunDto> Run(ITestRunAppService service, CreateTestRunDto input) => service.TriggerAsync(input);

    [McpServerTool(Name = PtnToolCodes.Result), Description("Read the persisted deterministic run report.")]
    public static Task<TestReportDetailDto> Result(ITestRunAppService service, Guid id) => service.GetReportAsync(id);

    [McpServerTool(Name = PtnToolCodes.Impact), Description("Measure the available write-set observation capability.")]
    public static Task<CapabilityLevelDto> Impact(IWriteSetCapabilityAppService service, Guid connectionId, bool hasExclusiveSandbox, CancellationToken cancellationToken) =>
        service.ProbeCapabilityAsync(connectionId, hasExclusiveSandbox, cancellationToken);

    [McpServerTool(Name = PtnToolCodes.DryRun), Description("Report a dry-run observation/contract contradiction without changing the verdict.")]
    public static Task<DryRunContradictionReportDto> DryRun(ITestRunAppService service, Guid id) => service.GetDryRunContradictionAsync(id);

    [McpServerTool(Name = PtnToolCodes.Task), Description("Map internal work and approval state to MCP Task status.")]
    public static Task<McpTaskStatusDto> TaskStatus(IPtnBridgeAppService service, McpTaskStatusRequestDto input) => service.MapTaskStatusAsync(input);

    [McpServerTool(Name = PtnToolCodes.Profile), Description("Resolve the tenant-scoped per-moment authoring profile.")]
    public static Task<AgentProfileDto> Profile(IPtnBridgeAppService service, AgentProfileRequestDto input) => service.ResolveAgentProfileAsync(input);
}
