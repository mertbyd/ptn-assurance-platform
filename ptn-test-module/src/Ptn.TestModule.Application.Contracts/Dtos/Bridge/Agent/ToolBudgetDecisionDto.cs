namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Anlik tool butcesinin kalan miktarlarini public tasir.
// sistemdeki gorevi: Basarili butce kapisinin makine-okur sonucudur.
public sealed class ToolBudgetDecisionDto
{
    public bool Allowed { get; set; }
    public int RemainingTurns { get; set; }
    public int RemainingTokens { get; set; }
}
