using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Yurutulen kanit yolunun hukum, kapsam, butce ve acik soru sonucunu tasir.
// sistemdeki gorevi: Unavailable ve NOT_BOUND durumlarini basarili teshis gibi gostermeden raporlar.
public sealed class PtnChainResult
{
    public string PathKey { get; set; } = string.Empty;
    public string VerdictCode { get; set; } = string.Empty;
    public PtnExplanationNode? Root { get; set; }
    public PtnCoverageReport Coverage { get; set; } = new();
    public int HopCount { get; set; }
    public bool BudgetExceeded { get; set; }
    public List<string> OpenQuestions { get; set; } = [];
}
