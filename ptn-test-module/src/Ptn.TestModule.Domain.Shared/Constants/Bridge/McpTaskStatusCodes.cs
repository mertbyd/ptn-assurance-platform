namespace Ptn.TestModule.Constants.Bridge;

// islevi: Ic durumlari MCP Task protokolunun kayipsiz durum sozlugune baglar.
// sistemdeki gorevi: Host ve Application sinirinin ayni wire kodlarini kullanmasini saglar.
public static class McpTaskStatusCodes
{
    public const string Working = "working";
    public const string InputRequired = "input_required";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
}
