using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: Database checker hipotezinin normalize edilmemis kaynak alanlarini tasir.
// sistemdeki gorevi: Kod ve bulgu referansi kararlarini Manager'a birakir.
public sealed class DatabaseDiagnosisHypothesis
{
    public string HypothesisKindCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public List<DatabaseDiagnosisEvidence> Evidence { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
