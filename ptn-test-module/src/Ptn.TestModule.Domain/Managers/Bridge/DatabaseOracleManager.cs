using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Database checker sonucunu normalize eder ve ham kanit degerlerini redakte eder.
// sistemdeki gorevi: Outcome karari ile veri sizintisi kuralini Application servisinden ayirir.
public class DatabaseOracleManager : DomainService
{
    // Checker projeksiyon capability'si olmadiginda kapali Unavailable sonucunu uretir.
    public PtnProjectionResult CreateUnavailableProjection() =>
        new() { StateCode = PtnEvidenceStateCodes.Unavailable };

    // Assertion sonucunu tek Bridge outcome koduna ve redaksiyonlu kanita cevirir.
    public PtnAssertionResult Normalize(PtnAssertionResult result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        result.RowSummary = Redact(result.RowSummary);
        result.FailedExpectations.ForEach(Redact);
        return result;
    }

    // Assertion listesinin her sonucuna ayni normalizasyon kurallarini uygular.
    public IReadOnlyList<PtnAssertionResult> Normalize(IReadOnlyList<PtnAssertionResult> results) =>
        results.Select(Normalize).ToList();

    // Checker outcome kodunu Bridge sozlugundeki karsiligina cevirir.
    private static string NormalizeOutcome(string outcomeCode) =>
        OutcomeMap.TryGetValue(outcomeCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);

    // Ham satir ozetindeki tum degerleri kapali redaksiyon koduyla degistirir.
    private static Dictionary<string, string?>? Redact(Dictionary<string, string?>? row) =>
        row?.ToDictionary(
            pair => pair.Key,
            pair => Redact(pair.Value),
            StringComparer.OrdinalIgnoreCase);

    // Tek failure kanitinin beklenen ve gozlenen degerlerini redakte eder.
    private static void Redact(PtnFailedExpectation failure)
    {
        failure.ExpectedValue = Redact(failure.ExpectedValue);
        failure.ObservedValue = Redact(failure.ObservedValue);
    }

    // Ham degeri ajana tasimadan kapali redaksiyon koduna cevirir.
    private static string? Redact(string? value) => value is null ? null : PtnRedactionCodes.Redacted;

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
}
