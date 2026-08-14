using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Constants.Comparison.Projections;
using DatabaseDerivabilityCodes = Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionDerivabilityCodes;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Database checker sonucunu normalize eder ve ham kanit degerlerini redakte eder.
// sistemdeki gorevi: Outcome karari ile veri sizintisi kuralini Application servisinden ayirir.
public class DatabaseOracleManager : TestModuleDomainService
{
    // Assertion sonucunu tek Bridge outcome koduna ve redaksiyonlu kanita cevirir.
    public AssertionResult Normalize(
        DatabaseAssertionRequest request,
        AssertionResult result)
    {
        if (!CorrelationMatches(request.Correlation, result.Correlation))
        {
            return CreateUnavailableAssertion(request.Correlation);
        }

        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        result.RowSummary = Redact(result.RowSummary);
        result.FailedExpectations.ForEach(Redact);
        return result;
    }

    // Assertion listesinin her sonucuna ayni normalizasyon kurallarini uygular.
    public IReadOnlyList<AssertionResult> Normalize(
        IReadOnlyList<DatabaseAssertionRequest> requests,
        IReadOnlyList<AssertionResult> results)
    {
        if (requests.Count != results.Count)
        {
            return requests.Select(request => CreateUnavailableAssertion(request.Correlation)).ToList();
        }

        return requests.Select((request, index) => Normalize(request, results[index])).ToList();
    }

    // Checker projection outcome'unu uc degerli kanit durumuna cevirir ve echo'yu dogrular.
    public ProjectionResult Normalize(
        ProjectionRequest request,
        ProjectionResult result)
    {
        if (!CorrelationMatches(request.Correlation, result.Correlation))
        {
            return CreateUnavailableProjection(request.Correlation);
        }

        result.StateCode = result.StateCode switch
        {
            ProjectionOutcomeCodes.Projected or ProjectionOutcomeCodes.Truncated =>
                result.ObservedRowCount > 0
                    ? PtnEvidenceStateCodes.Observed
                    : PtnEvidenceStateCodes.NotObserved,
            _ => PtnEvidenceStateCodes.Unavailable
        };
        if (result.StateCode == PtnEvidenceStateCodes.Unavailable)
        {
            result.Rows = [];
            result.ObservedRowCount = 0;
            result.Truncated = false;
        }

        return result;
    }

    // DB derivability outcome'larini Bridge sozlugune cevirip toplu yayin kapisini hesaplar.
    public DatabaseDerivabilityResult Normalize(DatabaseDerivabilityResult result)
    {
        result.Assertions.ForEach(item => item.OutcomeCode = NormalizeDerivability(item.OutcomeCode));
        result.AllDerivable = result.Assertions.Count > 0 &&
                              result.Assertions.All(item => item.OutcomeCode == PtnOutcomeCodes.Derivable);
        return result;
    }

    // Checker outcome kodunu Bridge sozlugundeki karsiligina cevirir.
    private static string NormalizeOutcome(string outcomeCode) =>
        OutcomeMap.TryGetValue(outcomeCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);

    // Checker DB derivability kodunu Bridge'in kapali outcome sozlugune cevirir.
    private static string NormalizeDerivability(string outcomeCode) =>
        DerivabilityMap.TryGetValue(outcomeCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);

    // Ham satir ozetindeki tum degerleri kapali redaksiyon koduyla degistirir.
    private static Dictionary<string, string?>? Redact(Dictionary<string, string?>? row) =>
        row?.ToDictionary(
            pair => pair.Key,
            pair => Redact(pair.Value),
            StringComparer.OrdinalIgnoreCase);

    // Tek failure kanitinin beklenen ve gozlenen degerlerini redakte eder.
    private static void Redact(FailedExpectation failure)
    {
        failure.ExpectedValue = Redact(failure.ExpectedValue);
        failure.ObservedValue = Redact(failure.ObservedValue);
    }

    // Ham degeri ajana tasimadan kapali redaksiyon koduna cevirir.
    private static string? Redact(string? value) => value is null ? null : PtnRedactionCodes.Redacted;

    // Istenen ve echo edilen korelasyon alanlarini ordinal olarak karsilastirir.
    private static bool CorrelationMatches(CorrelationRef? expected, CorrelationRef? actual) =>
        expected?.TraceId == actual?.TraceId && expected?.StepKey == actual?.StepKey;

    // Batch veya echo butunlugu bozuldugunda tek fail-closed assertion sonucu kurar.
    private static AssertionResult CreateUnavailableAssertion(CorrelationRef? correlation) => new()
    {
        OutcomeCode = PtnOutcomeCodes.Unavailable,
        Correlation = correlation
    };

    // Okunamayan projection'i yetki yorumu yapmadan kapali Unavailable sonucuna cevirir.
    private static ProjectionResult CreateUnavailableProjection(CorrelationRef? correlation) => new()
    {
        StateCode = PtnEvidenceStateCodes.Unavailable,
        Correlation = correlation
    };

    private static readonly IReadOnlyDictionary<string, string> OutcomeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AssertionOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [AssertionOutcomeCodes.RowNotFound] = PtnOutcomeCodes.RowNotFound,
            [AssertionOutcomeCodes.ValueMismatch] = PtnOutcomeCodes.ValueMismatch,
            [AssertionOutcomeCodes.CardinalityMismatch] = PtnOutcomeCodes.CardinalityMismatch,
            [AssertionOutcomeCodes.TimedOut] = PtnOutcomeCodes.TimedOut,
            [AssertionOutcomeCodes.KeyNotUnique] = PtnOutcomeCodes.KeyNotUnique,
            [AssertionOutcomeCodes.TableNotFound] = PtnOutcomeCodes.TableNotFound,
            [AssertionOutcomeCodes.ColumnNotFound] = PtnOutcomeCodes.ColumnNotFound
        };

    private static readonly IReadOnlyDictionary<string, string> DerivabilityMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DatabaseDerivabilityCodes.Derivable] = PtnOutcomeCodes.Derivable,
            [DatabaseDerivabilityCodes.TableNotFound] = PtnOutcomeCodes.TableNotFound,
            [DatabaseDerivabilityCodes.ColumnNotFound] = PtnOutcomeCodes.ColumnNotFound,
            [DatabaseDerivabilityCodes.KeyNotUnique] = PtnOutcomeCodes.KeyNotUnique,
            [DatabaseDerivabilityCodes.MatcherTypeMismatch] = PtnOutcomeCodes.MatcherTypeMismatch
        };
}
