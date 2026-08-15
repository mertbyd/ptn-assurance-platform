namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Ic is ve onay durumunu MCP Task eslemesine tasir.
// sistemdeki gorevi: taskId, TTL ve poll sozlesmesini tek public girdide toplar.
public sealed class McpTaskStatusRequestDto
{
    public string TaskId { get; set; } = string.Empty;
    public string InternalStatus { get; set; } = string.Empty;
    public bool ApprovalRequired { get; set; }
    public bool InfrastructureFailure { get; set; }
    public long TtlMs { get; set; }
    public long PollIntervalMs { get; set; }
}
