using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Tek hipotezin tur, oncelik, guven, lokalize metin, sinirli kanit ve sonraki kontrol sonucunu tasir.
// sistemdeki gorevi: Kural degerlendirmesini siralama ve RFC 9457 API raporuna aciklanabilir bir birim olarak verir.
public sealed class HypothesisAssessment
{
    public string HypothesisKindCode { get; set; }
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public List<ProbeEvidence> Evidence { get; set; }
    public List<string> NextChecks { get; set; } = new();

    // islevi: Kural sonucunu ortak alanlarla kurarak on hipotezde yinelenen initializer akisini tek noktada tutar.
    public HypothesisAssessment(
        string hypothesisKindCode,
        int priority,
        string confidenceCode,
        List<ProbeEvidence>? evidence = null)
    {
        HypothesisKindCode = hypothesisKindCode;
        Priority = priority;
        ConfidenceCode = confidenceCode;
        Evidence = evidence ?? new List<ProbeEvidence>();
    }
}
