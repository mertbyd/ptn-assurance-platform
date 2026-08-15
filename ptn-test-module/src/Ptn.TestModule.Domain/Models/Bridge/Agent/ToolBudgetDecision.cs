namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Tool cagrisinin anlik profil butcesinden gecip gecmedigini tasir.
// sistemdeki gorevi: Host'a kararli kalan tur ve token miktarini acar.
public class ToolBudgetDecision
{
    public bool Allowed { get; set; }
    public int RemainingTurns { get; set; }
    public int RemainingTokens { get; set; }
}
