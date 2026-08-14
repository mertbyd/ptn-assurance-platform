using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Bridge.Database;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using ApiHypothesisCodes = Ptn.ApiContractChecker.Constants.Diagnosis.HypothesisKindCodes;
using ApiProbeKindCodes = Ptn.ApiContractChecker.Constants.Diagnosis.ProbeKindCodes;
using DatabaseHypothesisCodes = Ptn.DatabaseChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseProbeKindCodes = Ptn.DatabaseChecker.Constants.Diagnosis.ProbeKindCodes;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Iki checker teshis gramerini ortak Bridge raporuna normalize eder.
// sistemdeki gorevi: Konum, outcome, hipotez, fact ve fingerprint kararlarini Application servisinden ayirir.
public class FailureDiagnosisManager : DomainService
{
    // Ortak Bridge sinyalinden API checker'a gidecek kaynak-ozgul modeli kurar.
    public PtnApiDiagnosisRequest CreateApiRequest(PtnDiagnosisRequest request) =>
        new()
        {
            SnapshotId = RequireSnapshotId(request.SpecSnapshotId),
            ContractCheckRunId = request.ApiRunId,
            OperationId = request.Location.OperationId,
            Method = request.Location.Method,
            Path = request.Location.Path,
            StatusCode = request.StatusCode,
            ContentType = request.ContentType,
            ConformanceOutcomeCode = ToApiOutcome(request.OutcomeCode),
            TransportErrorCode = request.TransportErrorCode,
            ObservedAtMs = request.ObservedAtMs
        };

    // Ortak Bridge sinyalinden Database checker'in dogru union kolunu kurar.
    public PtnDatabaseDiagnosisRequest CreateDatabaseRequest(PtnDiagnosisRequest request)
    {
        return new PtnDatabaseDiagnosisRequest
        {
            ConnectionId = request.ConnectionId,
            Assertion = string.IsNullOrWhiteSpace(request.OutcomeCode) ? null : CreateAssertion(request),
            DbException = string.IsNullOrWhiteSpace(request.OutcomeCode) ? CreateException(request) : null
        };
    }

    // API teshis raporuna kaynak, konum, identity olgulari ve normalize referanslari uygular.
    public PtnDiagnosisReport NormalizeApiReport(
        PtnDiagnosisReport report,
        PtnLocation location,
        PtnApiFailureIdentity identity)
    {
        report.SourceCheckerCode = PtnSourceCheckerCodes.ApiContract;
        report.Location = location;
        report.Facts = CreateApiFacts(identity);
        NormalizeHypotheses(report, ApiHypothesisMap, ApiFactMap);
        return report;
    }

    // Database teshis raporuna kaynak, konum ve normalize referanslari uygular.
    public PtnDiagnosisReport NormalizeDatabaseReport(PtnDiagnosisReport report, PtnLocation location)
    {
        report.SourceCheckerCode = PtnSourceCheckerCodes.DatabaseComparison;
        report.Location = location;
        NormalizeHypotheses(report, DatabaseHypothesisMap, DatabaseFactMap);
        return report;
    }

    // API checker konumunu ortak modelde API semasi olarak aciklar.
    public PtnLocation CreateApiLocation(
        string? schemaName,
        string? operationId,
        string? method,
        string? path,
        string? jsonPointer) =>
        new()
        {
            ApiSchemaName = schemaName,
            OperationId = operationId,
            Method = method,
            Path = path,
            JsonPointer = jsonPointer
        };

    // Database checker konumunu ortak modelde DB semasi ve tablosu olarak aciklar.
    public PtnLocation CreateDatabaseLocation(
        string? schemaName,
        string? tableName,
        string? columnName) =>
        new()
        {
            DbSchemaName = schemaName,
            DbTableName = tableName,
            ColumnName = columnName
        };

    // Nullable snapshot kimligini API checker'in zorunlu kimligine cevirir.
    public Guid RequireSnapshotId(Guid? snapshotId) => snapshotId ?? throw CheckerCallFailed();

    // Bridge outcome kodunu API checker'in kaynak koduna cevirir.
    public string? ToApiOutcome(string? code) => ReverseNormalize(code, ApiOutcomeMap);

    // Bridge outcome kodunu Database checker'in kaynak koduna cevirir.
    public string ToDatabaseOutcome(string? code) =>
        ReverseNormalize(code, DatabaseOutcomeMap) ?? throw CheckerCallFailed();

    // Ortak sinyalin database assertion kolunu kaynak-ozgul modele cevirir.
    private PtnDatabaseAssertionSignal CreateAssertion(PtnDiagnosisRequest request) =>
        new()
        {
            SchemaName = request.Location.DbSchemaName ?? string.Empty,
            TableName = request.Location.DbTableName ?? string.Empty,
            KeyValues = request.KeyValues,
            OutcomeCode = ToDatabaseOutcome(request.OutcomeCode),
            FailedExpectations = request.FailedExpectations
        };

    // Ortak sinyalin database exception kolunu kaynak-ozgul modele cevirir.
    private static PtnDatabaseExceptionSignal CreateException(PtnDiagnosisRequest request) =>
        new()
        {
            EngineCode = request.EngineCode ?? string.Empty,
            SqlState = request.SqlState ?? string.Empty,
            ProviderFields = request.ProviderFields
        };

    // Hipotez ve evidence kodlarini tek sozluge cevirip kaynakli fingerprint baglar.
    private static void NormalizeHypotheses(
        PtnDiagnosisReport report,
        IReadOnlyDictionary<string, string> hypothesisMap,
        IReadOnlyDictionary<string, string> factMap)
    {
        foreach (var hypothesis in report.Hypotheses)
        {
            hypothesis.HypothesisKindCode = Normalize(hypothesis.HypothesisKindCode, hypothesisMap);
            hypothesis.Evidence.ForEach(evidence => evidence.FactCode = Normalize(evidence.FactCode, factMap));
            hypothesis.Ref = CreateFindingRef(report.SourceCheckerCode, hypothesis, report.Location);
            hypothesis.Evidence.ForEach(evidence => evidence.Ref = hypothesis.Ref);
        }
    }

    // API identity'nin kanit yolunda kullanilan kapali olgu alanlarini ayiklar.
    private static Dictionary<string, List<string>> CreateApiFacts(PtnApiFailureIdentity identity) =>
        new(StringComparer.Ordinal)
        {
            [PtnDiagnosisFactCodes.ChallengeScopes] = identity.ChallengeScopes,
            [PtnDiagnosisFactCodes.AllowedMethods] = identity.AllowedMethods,
            [PtnDiagnosisFactCodes.StatusCode] = identity.StatusCode is null
                ? []
                : [identity.StatusCode.Value.ToString(CultureInfo.InvariantCulture)]
        };

    // Kaynak checker ve kanit govdesinden kararli sha256 bulgu referansi uretir.
    private static PtnFindingRef CreateFindingRef(
        string sourceCheckerCode,
        PtnDiagnosisHypothesis hypothesis,
        PtnLocation location)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            hypothesis.HypothesisKindCode,
            location.ApiSchemaName,
            location.DbSchemaName,
            location.DbTableName,
            hypothesis.Evidence
        });
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        return new PtnFindingRef
        {
            SourceCheckerCode = sourceCheckerCode,
            Fingerprint = PtnBridgeSettingNames.FingerprintPrefix + hash
        };
    }

    // Kaynak kodunu Bridge sozlugunde karsiligi yoksa kapali hata ile reddeder.
    private static string Normalize(string code, IReadOnlyDictionary<string, string> map) =>
        map.TryGetValue(code, out var normalized) ? normalized : throw CheckerCallFailed();

    // Bridge outcome kodunu ilgili checker'in kaynak koduna cevirir.
    private static string? ReverseNormalize(string? code, IReadOnlyDictionary<string, string> map) =>
        code is null
            ? null
            : map.SingleOrDefault(pair => pair.Value == code).Key ?? throw CheckerCallFailed();

    // Bilinmeyen checker gramerini ortak kodlu ABP hatasina cevirir.
    private static BusinessException CheckerCallFailed() => new(TestModuleBridgeErrorCodes.CheckerCallFailed);

    private static readonly IReadOnlyDictionary<string, string> ApiHypothesisMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ApiHypothesisCodes.ResponseSchemaChanged] = PtnHypothesisCodes.ResponseSchemaChanged,
            [ApiHypothesisCodes.RequiredRequestFieldCreated] = PtnHypothesisCodes.RequiredRequestFieldCreated,
            [ApiHypothesisCodes.EnumValueRemoved] = PtnHypothesisCodes.EnumValueRemoved,
            [ApiHypothesisCodes.EndpointRemovedOrMoved] = PtnHypothesisCodes.EndpointRemovedOrMoved,
            [ApiHypothesisCodes.SuccessStatusChanged] = PtnHypothesisCodes.SuccessStatusChanged,
            [ApiHypothesisCodes.MediaTypeRemoved] = PtnHypothesisCodes.MediaTypeRemoved,
            [ApiHypothesisCodes.PropertyBecameOptional] = PtnHypothesisCodes.PropertyBecameOptional,
            [ApiHypothesisCodes.ResourceNeverCreated] = PtnHypothesisCodes.ResourceNeverCreated,
            [ApiHypothesisCodes.ResourceCreatedLate] = PtnHypothesisCodes.ResourceCreatedLate,
            [ApiHypothesisCodes.AuthenticationMissing] = PtnHypothesisCodes.AuthenticationMissing,
            [ApiHypothesisCodes.TokenExpired] = PtnHypothesisCodes.TokenExpired,
            [ApiHypothesisCodes.InsufficientScope] = PtnHypothesisCodes.InsufficientScope,
            [ApiHypothesisCodes.PathNotDeployed] = PtnHypothesisCodes.PathNotDeployed,
            [ApiHypothesisCodes.MethodNotSupported] = PtnHypothesisCodes.MethodNotSupported,
            [ApiHypothesisCodes.SnapshotVersionMismatch] = PtnHypothesisCodes.SnapshotVersionMismatch,
            [ApiHypothesisCodes.AssertionValueDiffers] = PtnHypothesisCodes.AssertionValueDiffers,
            [ApiHypothesisCodes.AssertionRequiredFieldMissing] = PtnHypothesisCodes.AssertionRequiredFieldMissing,
            [ApiHypothesisCodes.AssertionOutsideContract] = PtnHypothesisCodes.AssertionOutsideContract,
            [ApiHypothesisCodes.VolatileLiteralAssertion] = PtnHypothesisCodes.VolatileLiteralAssertion
        };

    private static readonly IReadOnlyDictionary<string, string> DatabaseHypothesisMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DatabaseHypothesisCodes.RowNeverCreated] = PtnHypothesisCodes.ResourceNeverCreated,
            [DatabaseHypothesisCodes.RowCreatedLate] = PtnHypothesisCodes.ResourceCreatedLate,
            [DatabaseHypothesisCodes.RowValueDiffers] = PtnHypothesisCodes.AssertionValueDiffers,
            [DatabaseHypothesisCodes.RowInAnotherScope] = PtnHypothesisCodes.RowInAnotherScope,
            [DatabaseHypothesisCodes.ExpectedColumnMissing] = PtnHypothesisCodes.ExpectedColumnMissing,
            [DatabaseHypothesisCodes.ForeignKeyParentMissing] = PtnHypothesisCodes.ForeignKeyParentMissing,
            [DatabaseHypothesisCodes.ConstraintNotValidated] = PtnHypothesisCodes.ConstraintNotValidated,
            [DatabaseHypothesisCodes.UniqueDuplicateExists] = PtnHypothesisCodes.UniqueDuplicateExists,
            [DatabaseHypothesisCodes.GeneratedColumnWrite] = PtnHypothesisCodes.GeneratedColumnWrite,
            [DatabaseHypothesisCodes.ServerSettingMismatch] = PtnHypothesisCodes.ServerSettingMismatch
        };

    private static readonly IReadOnlyDictionary<string, string> ApiFactMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ApiProbeKindCodes.Facts.Present] = PtnFactCodes.Present,
            [ApiProbeKindCodes.Facts.Absent] = PtnFactCodes.Absent,
            [ApiProbeKindCodes.Facts.Match] = PtnFactCodes.Match,
            [ApiProbeKindCodes.Facts.Mismatch] = PtnFactCodes.Mismatch,
            [ApiProbeKindCodes.Facts.Reachable] = PtnFactCodes.Reachable,
            [ApiProbeKindCodes.Facts.Unreachable] = PtnFactCodes.Unreachable,
            [ApiProbeKindCodes.Facts.TimedOut] = PtnFactCodes.TimedOut
        };

    private static readonly IReadOnlyDictionary<string, string> DatabaseFactMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DatabaseProbeKindCodes.Facts.Found] = PtnFactCodes.Found,
            [DatabaseProbeKindCodes.Facts.Missing] = PtnFactCodes.Missing,
            [DatabaseProbeKindCodes.Facts.Matches] = PtnFactCodes.Match,
            [DatabaseProbeKindCodes.Facts.Mismatch] = PtnFactCodes.Mismatch,
            [DatabaseProbeKindCodes.Facts.Catalog] = PtnFactCodes.Catalog
        };

    private static readonly IReadOnlyDictionary<string, string> ApiOutcomeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.StatusCodeUndocumented] = PtnOutcomeCodes.StatusCodeUndocumented,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.MediaTypeUndocumented] = PtnOutcomeCodes.MediaTypeUndocumented,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.ResponseSchemaViolation] = PtnOutcomeCodes.ResponseSchemaViolation,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.RequiredHeaderMissing] = PtnOutcomeCodes.RequiredHeaderMissing,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.UndocumentedProperty] = PtnOutcomeCodes.UndocumentedProperty,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.ServerError] = PtnOutcomeCodes.ServerError,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.OperationNotResolved] = PtnOutcomeCodes.OperationNotResolved,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.SnapshotNotFound] = PtnOutcomeCodes.SnapshotNotFound,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.PolicySuppressed] = PtnOutcomeCodes.PolicySuppressed,
            [Ptn.ApiContractChecker.Constants.Conformance.Lookups.ConformanceOutcomeCodes.SchemaNotResolved] = PtnOutcomeCodes.SchemaNotResolved
        };

    private static readonly IReadOnlyDictionary<string, string> DatabaseOutcomeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.RowNotFound] = PtnOutcomeCodes.RowNotFound,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.ValueMismatch] = PtnOutcomeCodes.ValueMismatch,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.CardinalityMismatch] = PtnOutcomeCodes.CardinalityMismatch,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.TimedOut] = PtnOutcomeCodes.TimedOut,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.KeyNotUnique] = PtnOutcomeCodes.KeyNotUnique,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.TableNotFound] = PtnOutcomeCodes.TableNotFound,
            [Ptn.DatabaseChecker.Constants.Comparison.Assertions.AssertionOutcomeCodes.ColumnNotFound] = PtnOutcomeCodes.ColumnNotFound
        };
}
