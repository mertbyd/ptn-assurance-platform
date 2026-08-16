using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Kapali outcome kodu ile deger icermeyen sirali ihlalleri tasir.
// sistemdeki gorevi: Test Module'a 512 bayt siniri icinde deterministik oracle sonucu verir.
public sealed class ResponseConformanceResult
{
    public string OutcomeCode { get; }
    public List<ConformanceViolation> Violations { get; }
    public CorrelationRef? Correlation { get; }

    public ResponseConformanceResult(string outcomeCode, List<ConformanceViolation> violations)
        : this(outcomeCode, violations, null)
    {
    }

    public ResponseConformanceResult(
        string outcomeCode,
        List<ConformanceViolation> violations,
        CorrelationRef? correlation)
    {
        OutcomeCode = outcomeCode;
        Violations = violations;
        Correlation = correlation;
    }

    // Once ayar sayisina, sonra gercek UTF-8 butcesine gore sondaki ihlalleri kirpar.
    public void TrimToBudget(int maxViolations, int maxResponseBytes)
    {
        if (Violations.Count > maxViolations)
        {
            Violations.RemoveRange(maxViolations, Violations.Count - maxViolations);
        }

        var safeBudget = Math.Max(
            0,
            maxResponseBytes - ValueRetentionConstants.TransportSerializationMarginBytes);
        while (MeasureUtf8Bytes() > safeBudget && Violations.Count > 0)
        {
            Violations.RemoveAt(Violations.Count - 1);
        }
    }

    public int MeasureUtf8Bytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this).Length;
    }
}
