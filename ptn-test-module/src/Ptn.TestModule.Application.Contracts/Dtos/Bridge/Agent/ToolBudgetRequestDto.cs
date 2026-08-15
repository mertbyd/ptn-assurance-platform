namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Anlik tool cagrisinin harcadigi tur ve token miktarini tasir.
// sistemdeki gorevi: Butce asimini checker veya model cagrisindan once reddettirir.
public sealed class ToolBudgetRequestDto
{
    public string MomentCode { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public int UsedTurns { get; set; }
    public int UsedTokens { get; set; }
}
