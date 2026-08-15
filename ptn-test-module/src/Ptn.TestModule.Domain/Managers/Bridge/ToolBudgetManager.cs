using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Tek an icindeki tool, tur ve token harcamasini profil tavanina karsi denetler.
// sistemdeki gorevi: Butce asimini mevcut ToolBudgetExceeded koduyla deterministik reddeder.
public class ToolBudgetManager : TestModuleDomainService
{
    // Cagriyi izinli tool alt kumesi ve iki sayac tavanina karsi denetler.
    public ToolBudgetDecision EnsureWithinBudget(AgentProfile profile, string toolCode, int usedTurns, int usedTokens)
    {
        if (!profile.AllowedToolCodes.Contains(toolCode) || usedTurns >= profile.MaxTurns || usedTokens >= profile.TokenLimit)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ToolBudgetExceeded);
        }

        return new ToolBudgetDecision
        {
            Allowed = true,
            RemainingTurns = profile.MaxTurns - usedTurns - 1,
            RemainingTokens = profile.TokenLimit - usedTokens
        };
    }
}
