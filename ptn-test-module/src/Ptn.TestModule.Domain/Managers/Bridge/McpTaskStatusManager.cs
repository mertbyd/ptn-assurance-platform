using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Models.Bridge.Agent;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Ic kosum ve onay durumlarini MCP Task durumlarina kayipsiz esler.
// sistemdeki gorevi: Onay beklemeyi input_required, altyapi hatasini failed olarak ayirir.
public class McpTaskStatusManager : TestModuleDomainService
{
    // Ic durum ve bayraklardan wire task durumunu kurar.
    public McpTaskStatus Map(string taskId, string internalStatus, bool approvalRequired, bool infrastructureFailure, long ttlMs, long pollIntervalMs)
    {
        var status = approvalRequired ? McpTaskStatusCodes.InputRequired : internalStatus switch
        {
            TestRunStatusCodes.Pending or TestRunStatusCodes.Running => McpTaskStatusCodes.Working,
            TestRunStatusCodes.Cancelled => McpTaskStatusCodes.Cancelled,
            TestRunStatusCodes.Completed when !infrastructureFailure => McpTaskStatusCodes.Completed,
            _ => McpTaskStatusCodes.Failed
        };
        return new McpTaskStatus { TaskId = taskId, Status = status, TtlMs = ttlMs, PollIntervalMs = pollIntervalMs };
    }
}
