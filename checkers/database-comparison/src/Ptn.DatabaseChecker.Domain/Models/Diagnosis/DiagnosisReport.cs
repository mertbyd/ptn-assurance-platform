using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: RFC 9457 alanlari ile checknexus kimlik, konum, sirali hipotez ve sonraki kontrol uzantilarini tasir.
// sistemdeki gorevi: Deterministik teshis sonucunu kalici entity olmadan 4 KB UTF-8 butcesi icinde Test Module'a dondurur.
public sealed class DiagnosisReport
{
    private const int TransportSerializationMarginBytes = 512;

    public string Type { get; set; } = FailureSourceKindCodes.Report.Type;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; } = FailureSourceKindCodes.Report.Status;
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = FailureSourceKindCodes.Report.Instance;
    public FailureIdentity Identity { get; set; } = new();
    public ObjectReference Location { get; set; } = new();
    public List<HypothesisAssessment> Hypotheses { get; set; } = new();
    public List<string> NextChecks { get; set; } = new();
    public CorrelationRef? Correlation { get; set; }

    // islevi: Raporu once next-check/kanit, sonra dusuk sirali hipotezlerden kirparak UTF-8 tavanina indirir.
    public void TrimToBudget()
    {
        TrimNextChecks();
        TrimEvidence();
        while (ExceedsSafeBudget() && Hypotheses.Count > 1)
        {
            Hypotheses.RemoveAt(Hypotheses.Count - 1);
        }

        if (ExceedsSafeBudget())
        {
            Detail = string.Empty;
            Hypotheses.ForEach(item => item.Detail = string.Empty);
        }
    }

    // islevi: Top-level ve hipotez next-check listelerini azami uc ogeyle baslatip gerekirse sondan azaltir.
    private void TrimNextChecks()
    {
        NextChecks = NextChecks.Take(FailureSourceKindCodes.Report.MaxNextChecks).ToList();
        Hypotheses.ForEach(item => item.NextChecks = item.NextChecks
            .Take(FailureSourceKindCodes.Report.MaxNextChecks).ToList());
        while (ExceedsSafeBudget() && NextChecks.Count > 0)
        {
            NextChecks.RemoveAt(NextChecks.Count - 1);
        }
    }

    // islevi: Her hipotezin kanitini uc ogeyle sinirlar ve butce asiminda dusuk siradan kanit eksiltir.
    private void TrimEvidence()
    {
        Hypotheses.ForEach(item => item.Evidence = item.Evidence
            .Take(FailureSourceKindCodes.Report.MaxEvidencePerHypothesis).ToList());
        foreach (var hypothesis in Hypotheses.AsEnumerable().Reverse())
        {
            while (ExceedsSafeBudget() && hypothesis.Evidence.Count > 0)
            {
                hypothesis.Evidence.RemoveAt(hypothesis.Evidence.Count - 1);
            }
        }
    }

    // islevi: Domain raporunun JSON UTF-8 govde boyutunu deterministik olarak olcer.
    public int MeasureUtf8Bytes()
        => JsonSerializer.SerializeToUtf8Bytes(this).Length;

    // islevi: Mapper/JSON extension alan adlari icin emniyet payi birakarak transport tavaninin asilmasini onler.
    private bool ExceedsSafeBudget()
        => MeasureUtf8Bytes() >
           FailureSourceKindCodes.Report.MaxUtf8Bytes - TransportSerializationMarginBytes;
}
