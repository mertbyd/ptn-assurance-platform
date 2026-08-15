namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Uzun is durumunu MCP Task wire alanlariyla tasir.
// sistemdeki gorevi: Onay bekleme ve altyapi hatasini tamamlanmis isten ayirir.
public class McpTaskStatus
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long TtlMs { get; set; }
    public long PollIntervalMs { get; set; }
}
