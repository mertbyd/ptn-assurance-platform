using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit zincirinin tek gozlem adimini, konumunu ve alt adimlarini tasir.
// sistemdeki gorevi: Neden raporunu ikinci bir akil yurutme yerine yurutulen adimlarin kaydi yapar.
public sealed class ExplanationNode
{
    public string NodeKindCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string RelevanceCode { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
    public List<Evidence> Evidence { get; set; } = [];
    public List<ExplanationNode> Children { get; set; } = [];
}
