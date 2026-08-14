using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Kanit zinciri hukmunu, kapsami ve kapali sorulari response bicimiyle tasir.
// sistemdeki gorevi: ptn_explain yanitinda karar ve kritik olguyu agir aciklama agacindan once sunar.
public sealed class PtnExplainResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReport Coverage { get; set; } = new();
    public string VerdictCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public PtnExplanationNode? Root { get; set; }
    public List<PtnClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
