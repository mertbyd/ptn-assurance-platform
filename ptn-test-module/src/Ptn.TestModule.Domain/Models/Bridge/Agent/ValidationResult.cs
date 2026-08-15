using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Assertion turetilebilirligi ve yayin kapisi kararini tek kapali sonuc olarak tasir.
// sistemdeki gorevi: Eksik referans veya spec boslugunda makul ama yanlis Allow karari uretilmesini engeller.
public sealed class ValidationResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public CoverageReport Coverage { get; set; } = new();
    public bool IsPublishable { get; set; }
    public string DecisionCode { get; set; } = string.Empty;
    public DerivabilityResult? Derivability { get; set; }
    public DatabaseDerivabilityResult? DatabaseDerivability { get; set; }
    public List<ClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
