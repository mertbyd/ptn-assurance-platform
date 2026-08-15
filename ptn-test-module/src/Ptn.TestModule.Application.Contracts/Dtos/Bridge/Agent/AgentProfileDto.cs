namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Ajan aninin izinli tool ve butce profilini public tasir.
// sistemdeki gorevi: Tenant setting snapshot'ini MCP yuzeyine tipli acar.
public sealed class AgentProfileDto
{
    public string MomentCode { get; set; } = string.Empty;
    public List<string> AllowedToolCodes { get; set; } = [];
    public int MaxTurns { get; set; }
    public int TokenLimit { get; set; }
}
