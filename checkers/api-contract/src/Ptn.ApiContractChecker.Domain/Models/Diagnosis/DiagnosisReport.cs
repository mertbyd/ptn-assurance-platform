using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: RFC 9457 alanlariyla kimlik, konum, sirali hipotez ve tipli sonraki kontrolleri tasir.
// sistemdeki gorevi: Kalici entity acmadan deterministik teshisi 4 KB UTF-8 butcesinde dondurur.
public sealed class DiagnosisReport
{
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

    // islevi: Raporu nextChecks, kanit, dusuk sirali hipotez ve detail sirasiyla UTF-8 tavanina kirpar.
    public void TrimToBudget()
    {
        LimitCollections();
        TrimNextChecks();
        TrimEvidence();
        TrimHypotheses();
        TrimDetail();
    }

    // islevi: JSON UTF-8 govde boyutunu ayni serializer ile deterministik olcer.
    public int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;

    private void LimitCollections()
    {
        NextChecks = NextChecks.Take(FailureSourceKindCodes.Report.MaxNextChecks).ToList();
        Hypotheses.ForEach(item =>
        {
            item.NextChecks = item.NextChecks.Take(FailureSourceKindCodes.Report.MaxNextChecks).ToList();
            item.Evidence = item.Evidence.Take(FailureSourceKindCodes.Report.MaxEvidencePerHypothesis).ToList();
        });
    }

    private void TrimNextChecks()
    {
        while (ExceedsSafeBudget() && NextChecks.Count > 0)
        {
            NextChecks.RemoveAt(NextChecks.Count - 1);
        }

        foreach (var hypothesis in Hypotheses.AsEnumerable().Reverse())
        {
            while (ExceedsSafeBudget() && hypothesis.NextChecks.Count > 0)
            {
                hypothesis.NextChecks.RemoveAt(hypothesis.NextChecks.Count - 1);
            }
        }
    }

    private void TrimEvidence()
    {
        foreach (var hypothesis in Hypotheses.AsEnumerable().Reverse())
        {
            while (ExceedsSafeBudget() && hypothesis.Evidence.Count > 0)
            {
                hypothesis.Evidence.RemoveAt(hypothesis.Evidence.Count - 1);
            }
        }
    }

    private void TrimHypotheses()
    {
        while (ExceedsSafeBudget() && Hypotheses.Count > 1)
        {
            Hypotheses.RemoveAt(Hypotheses.Count - 1);
        }
    }

    private void TrimDetail()
    {
        if (!ExceedsSafeBudget())
        {
            return;
        }

        Detail = string.Empty;
        Hypotheses.ForEach(item => item.Detail = string.Empty);
    }

    private bool ExceedsSafeBudget()
        => MeasureUtf8Bytes() > FailureSourceKindCodes.Report.MaxUtf8Bytes -
           FailureSourceKindCodes.Report.SerializationMarginBytes;
}
