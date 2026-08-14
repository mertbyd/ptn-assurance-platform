using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiAssertionDerivabilityCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.AssertionDerivabilityCodes;
using ApiConformanceOutcomeCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes;
using ApiDiagnosisConfidenceCodes = Ptn.ApiContractChecker.Constants.Diagnosis.DiagnosisConfidenceCodes;
using ApiHypothesisKindCodes = Ptn.ApiContractChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseAssertionOutcomeCodes = Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes;
using DatabaseDiagnosisConfidenceCodes = Ptn.DatabaseChecker.Constants.Diagnosis.DiagnosisConfidenceCodes;
using DatabaseHypothesisKindCodes = Ptn.DatabaseChecker.Constants.Diagnosis.HypothesisKindCodes;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Koprunun normalize ettigi checker kod kumelerini kaynak paket sabitlerine karsi kilitler.
// sistemdeki gorevi: Checker yeni kod yayimladiginda sessiz sozluk drift'i yerine kirmizi test uretir.
public class VocabularyDriftTests
{
    // API uygunluk ve turetilebilirlik outcome kumelerini beklenen paket sozlesmesiyle karsilastirir.
    [Fact]
    public void Api_outcome_vocabulary_should_not_drift()
    {
        ApiConformanceOutcomeCodes.All.ShouldBe(ApiOutcomes, ignoreOrder: true);
        ApiAssertionDerivabilityCodes.All.ShouldBe(ApiDerivabilityOutcomes, ignoreOrder: true);
    }

    // Database assertion outcome kumesini beklenen paket sozlesmesiyle karsilastirir.
    [Fact]
    public void Database_outcome_vocabulary_should_not_drift()
    {
        ReadConstantValues(typeof(DatabaseAssertionOutcomeCodes))
            .ShouldBe(DatabaseOutcomes, ignoreOrder: true);
    }

    // Iki checker'in hipotez ve guven kumelerini beklenen paket sozlesmesiyle karsilastirir.
    [Fact]
    public void Diagnosis_vocabulary_should_not_drift()
    {
        ReadConstantValues(typeof(ApiHypothesisKindCodes)).ShouldBe(ApiHypotheses, ignoreOrder: true);
        ReadConstantValues(typeof(DatabaseHypothesisKindCodes)).ShouldBe(DatabaseHypotheses, ignoreOrder: true);
        ReadConstantValues(typeof(ApiDiagnosisConfidenceCodes)).ShouldBe(ConfidenceCodes, ignoreOrder: true);
        ReadConstantValues(typeof(DatabaseDiagnosisConfidenceCodes)).ShouldBe(ConfidenceCodes, ignoreOrder: true);
    }

    // Public string sabitlerinin degerlerini reflection ile kapali bir kumeye cevirir.
    private static IReadOnlyCollection<string> ReadConstantValues(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
    }

    private static readonly string[] ApiOutcomes =
    [
        "passed", "status-code-undocumented", "media-type-undocumented",
        "response-schema-violation", "required-header-missing", "undocumented-property",
        "server-error", "operation-not-resolved", "snapshot-not-found", "policy-suppressed",
        "schema-not-resolved"
    ];

    private static readonly string[] ApiDerivabilityOutcomes =
        ["Derivable", "AssertionNotInContract", "DerivableButOptional"];

    private static readonly string[] DatabaseOutcomes =
    [
        "Passed", "RowNotFound", "ValueMismatch", "CardinalityMismatch", "TimedOut",
        "KeyNotUnique", "TableNotFound", "ColumnNotFound"
    ];

    private static readonly string[] ApiHypotheses =
    [
        "H-CD-01", "H-CD-02", "H-CD-03", "H-CD-04", "H-CD-05", "H-CD-06", "H-CD-07",
        "H-ST-01", "H-ST-02", "H-AU-01", "H-AU-02", "H-AU-03", "H-EN-01", "H-EN-02",
        "H-EN-04", "H-AS-01", "H-AS-02", "H-AS-03", "H-AS-04"
    ];

    private static readonly string[] DatabaseHypotheses =
    [
        "RowNeverCreated", "RowCreatedLate", "RowValueDiffers", "RowInAnotherScope",
        "ExpectedColumnMissing", "ForeignKeyParentMissing", "ConstraintNotValidated",
        "UniqueDuplicateExists", "GeneratedColumnWrite", "ServerSettingMismatch"
    ];

    private static readonly string[] ConfidenceCodes =
        ["High", "Low", "Confirmed", "Likely", "Possible", "RuledOut"];
}
