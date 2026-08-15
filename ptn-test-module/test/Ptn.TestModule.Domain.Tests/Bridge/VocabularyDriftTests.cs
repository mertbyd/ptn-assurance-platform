using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using ApiAssertionDerivabilityCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.AssertionDerivabilityCodes;
using ApiConformanceOutcomeCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes;
using ApiDiagnosisConfidenceCodes = Ptn.ApiContractChecker.Constants.Diagnosis.DiagnosisConfidenceCodes;
using ApiHypothesisKindCodes = Ptn.ApiContractChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseAssertionOutcomeCodes = Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes;
using DatabaseDiagnosisConfidenceCodes = Ptn.DatabaseChecker.Constants.Diagnosis.DiagnosisConfidenceCodes;
using DatabaseHypothesisKindCodes = Ptn.DatabaseChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseFootprintStrengthCodes = Ptn.DatabaseChecker.Constants.Capabilities.FootprintStrengthCodes;
using DatabaseCapabilityReasonCodes = Ptn.DatabaseChecker.Constants.Capabilities.CapabilityReasonCodes;
using DatabaseProjectionOutcomeCodes = Ptn.DatabaseChecker.Constants.Comparison.Projections.ProjectionOutcomeCodes;
using DatabaseAssertionDerivabilityCodes = Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionDerivabilityCodes;
using ApiOperationLinkSourceCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.OperationLinkSourceCodes;
using ApiConformanceProfileCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceProfileCodes;
using DatabaseForeignKeyDirectionCodes = Ptn.DatabaseChecker.Constants.Comparison.ForeignKeyDirectionCodes;
using DatabaseSchemaLintWarningCodes = Ptn.DatabaseChecker.Constants.Comparison.SchemaLintWarningCodes;
using ApiSampleKindCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.SampleKindCodes;
using ApiSamplePositionCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.SamplePositionCodes;
using ApiSampleExpectedOutcomeCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.SampleExpectedOutcomeCodes;
using ApiConstraintCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConstraintCodes;
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

    // Database capability, projection ve derivability kod kumelerini yayimlanan sozlesmeye kilitler.
    [Fact]
    public void Database_bridge_surface_vocabulary_should_not_drift()
    {
        DatabaseFootprintStrengthCodes.All.ShouldBe(FootprintStrengths, ignoreOrder: true);
        DatabaseCapabilityReasonCodes.All.ShouldBe(CapabilityReasons, ignoreOrder: true);
        DatabaseProjectionOutcomeCodes.All.ShouldBe(ProjectionOutcomes, ignoreOrder: true);
        DatabaseAssertionDerivabilityCodes.All.ShouldBe(DatabaseDerivabilityOutcomes, ignoreOrder: true);
    }

    // API sample ve operation-link kod kumelerini yayimlanan sozlesmeye kilitler.
    [Fact]
    public void Api_authoring_surface_vocabulary_should_not_drift()
    {
        ApiOperationLinkSourceCodes.All.ShouldBe(OperationLinkSources, ignoreOrder: true);
        PtnOperationLinkSourceCodes.All.ShouldBe(ApiOperationLinkSourceCodes.All, ignoreOrder: true);
        ApiSampleKindCodes.All.ShouldBe(SampleKinds, ignoreOrder: true);
        ApiSamplePositionCodes.All.ShouldBe(SamplePositions, ignoreOrder: true);
        ApiSampleExpectedOutcomeCodes.All.ShouldBe(SampleExpectedOutcomes, ignoreOrder: true);
        ApiConstraintCodes.All.ShouldBe(ConstraintKinds, ignoreOrder: true);
    }

    // Yeni DB yazarlik ve API profil sozluklerini yayimlanan checker paketlerine birebir kilitler.
    [Fact]
    public void Bridge_authoring_vocabulary_should_mirror_published_checker_codes()
    {
        PtnForeignKeyDirectionCodes.All.ShouldBe(
            ReadConstantValues(typeof(DatabaseForeignKeyDirectionCodes)),
            ignoreOrder: true);
        PtnSchemaLintWarningCodes.All.ShouldBe(DatabaseSchemaLintWarningCodes.All, ignoreOrder: true);
        PtnConformanceProfileCodes.All.ShouldBe(ApiConformanceProfileCodes.All, ignoreOrder: true);
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

    private static readonly string[] FootprintStrengths =
        ["Exact", "RowAddressed", "Inferred", "Unavailable"];

    private static readonly string[] CapabilityReasons =
    [
        "SharedEnvironment", "WalLevelNotLogical", "NoReplicationGrant",
        "EngineNotSupported", "NoCapability", "SlotReleaseFailed"
    ];

    private static readonly string[] ProjectionOutcomes =
        ["Projected", "TableNotFound", "ColumnNotFound", "KeyNotUnique", "NotAuthorized", "Truncated"];

    private static readonly string[] DatabaseDerivabilityOutcomes =
        ["Derivable", "TableNotFound", "ColumnNotFound", "KeyNotUnique", "MatcherTypeMismatch"];

    private static readonly string[] OperationLinkSources =
        ["DeclaredLink", "SchemaMatch", "LocationHeader"];

    private static readonly string[] SampleKinds = ["Boundary", "Negative", "Both"];

    private static readonly string[] SamplePositions =
        ["BelowMin", "AtMin", "AboveMin", "BelowMax", "AtMax", "AboveMax", "Violation"];

    private static readonly string[] SampleExpectedOutcomes = ["ShouldAccept", "ShouldReject"];

    private static readonly string[] ConstraintKinds =
        ["MinLength", "MaxLength", "Minimum", "Maximum", "Pattern", "Enum", "Required", "Type", "Format"];
}
