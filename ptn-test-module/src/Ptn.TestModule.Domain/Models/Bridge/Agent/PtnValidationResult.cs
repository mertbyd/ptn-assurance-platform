using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Assertion turetilebilirligi ve yayin kapisi kararini tek kapali sonuc olarak tasir.
// sistemdeki gorevi: Eksik referans veya spec boslugunda makul ama yanlis Allow karari uretilmesini engeller.
public sealed class PtnValidationResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReport Coverage { get; set; } = new();
    public bool IsPublishable { get; set; }
    public string DecisionCode { get; set; } = string.Empty;
    public PtnDerivabilityResult? Derivability { get; set; }
    public List<PtnClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
