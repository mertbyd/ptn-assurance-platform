using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Kanit zinciri hukmunu, kapsami ve kapali sorulari response bicimiyle tasir.
// sistemdeki gorevi: ptn_explain yanitinda karar ve kritik olguyu agir aciklama agacindan once sunar.
public sealed class ExplainResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public CoverageReport Coverage { get; set; } = new();
    public string VerdictCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public ExplanationNode? Root { get; set; }
    public List<ClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
