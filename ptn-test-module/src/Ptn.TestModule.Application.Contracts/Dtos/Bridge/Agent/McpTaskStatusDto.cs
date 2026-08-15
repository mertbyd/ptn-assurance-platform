namespace Ptn.TestModule.Dtos.Bridge;

// islevi: MCP Task durumunu protokol alanlariyla public tasir.
// sistemdeki gorevi: Uzun is polling istemcisinin kayipsiz wire cevabidir.
public sealed class McpTaskStatusDto
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long TtlMs { get; set; }
    public long PollIntervalMs { get; set; }
}
