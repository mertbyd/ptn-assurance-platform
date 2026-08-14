using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;
using ApiDiagnoseRequestDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using ApiDiagnosisReportDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnosisReportDto;
using ApiDiagnosisService = Ptn.ApiContractChecker.Services.Diagnosis.IDiagnosisAppService;
using ApiHypothesisCodes = Ptn.ApiContractChecker.Constants.Diagnosis.HypothesisKindCodes;
using ApiProbeKindCodes = Ptn.ApiContractChecker.Constants.Diagnosis.ProbeKindCodes;
using DatabaseDiagnoseRequestDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using DatabaseDiagnosisReportDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnosisReportDto;
using DatabaseDiagnosisService = Ptn.DatabaseChecker.Services.Diagnosis.IDiagnosisAppService;
using DatabaseHypothesisCodes = Ptn.DatabaseChecker.Constants.Diagnosis.HypothesisKindCodes;
using DatabaseProbeKindCodes = Ptn.DatabaseChecker.Constants.Diagnosis.ProbeKindCodes;
using FailedExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto;

namespace Ptn.TestModule.Adapters;

// islevi: Iki checker teshis DTO'sunu kaynakli, normalize ve kanit referansli tek kopru raporuna cevirir.
// sistemdeki gorevi: SchemaName ve hipotez grameri cakismalarini domain ve ajan yuzeyinden gizler.
public class FailureDiagnosisAdapter : IFailureDiagnosisPort
{
    private readonly ApiDiagnosisService _apiDiagnosisService;
    private readonly DatabaseDiagnosisService _databaseDiagnosisService;

    // Iki checker public teshis AppService'ini yalniz anti-corruption adapter'inda birlestirir.
    public FailureDiagnosisAdapter(
        ApiDiagnosisService apiDiagnosisService,
        DatabaseDiagnosisService databaseDiagnosisService)
    {
        _apiDiagnosisService = apiDiagnosisService;
        _databaseDiagnosisService = databaseDiagnosisService;
    }

    // Ortak teshis girdisini API checker DTO'suna cevirip kaynakli kopru raporu dondurur.
    public async Task<PtnDiagnosisReport> DiagnoseApiAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _apiDiagnosisService.DiagnoseAsync(new ApiDiagnoseRequestDto
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
        });
        return MapApiReport(result);
    }

    // Ortak teshis girdisini database checker sinyaline cevirip kaynakli kopru raporu dondurur.
    public async Task<PtnDiagnosisReport> DiagnoseDatabaseAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _databaseDiagnosisService.DiagnoseAsync(
            MapDatabaseRequest(request),
            cancellationToken);
        return MapDatabaseReport(result);
    }

    // API checker raporunu ApiSchemaName semantigi ve normalize hipotezlerle kopruye cevirir.
    private static PtnDiagnosisReport MapApiReport(ApiDiagnosisReportDto report)
    {
        var location = new PtnLocation
        {
            ApiSchemaName = report.Location.SchemaName,
            OperationId = report.Location.OperationId,
            Method = report.Location.Method,
            Path = report.Location.Path,
            JsonPointer = report.Location.JsonPointer
        };
        return new PtnDiagnosisReport
        {
            SourceCheckerCode = PtnSourceCheckerCodes.ApiContract,
            Type = report.Type,
            Title = report.Title,
            Status = report.Status,
            Detail = report.Detail,
            Instance = report.Instance,
            Location = location,
            Facts = new Dictionary<string, List<string>>(StringComparer.Ordinal)
            {
                [PtnDiagnosisFactCodes.ChallengeScopes] = report.Identity.ChallengeScopes,
                [PtnDiagnosisFactCodes.AllowedMethods] = report.Identity.AllowedMethods,
                [PtnDiagnosisFactCodes.StatusCode] = report.Identity.StatusCode is null
                    ? []
                    : [report.Identity.StatusCode.Value.ToString(CultureInfo.InvariantCulture)]
            },
            Hypotheses = report.Hypotheses.Select(item => MapApiHypothesis(item, location)).ToList(),
            NextChecks = report.NextChecks
        };
    }

    // Database checker raporunu DbSchemaName semantigi ve normalize hipotezlerle kopruye cevirir.
    private static PtnDiagnosisReport MapDatabaseReport(DatabaseDiagnosisReportDto report)
    {
        var location = new PtnLocation
        {
            DbSchemaName = report.Location.SchemaName,
            DbTableName = report.Location.TableName,
            ColumnName = report.Location.ColumnName
        };
        return new PtnDiagnosisReport
        {
            SourceCheckerCode = PtnSourceCheckerCodes.DatabaseComparison,
            Type = report.Type,
            Title = report.Title,
            Status = report.Status,
            Detail = report.Detail,
            Instance = report.Instance,
            Location = location,
            Hypotheses = report.Hypotheses.Select(item => MapDatabaseHypothesis(item, location)).ToList(),
            NextChecks = report.NextChecks
        };
    }

    // API hipotezini tek gramer, kanit olgulari ve kaynakli fingerprint ile cevirir.
    private static PtnDiagnosisReport.PtnDiagnosisHypothesis MapApiHypothesis(
        Ptn.ApiContractChecker.Dtos.Diagnosis.HypothesisAssessmentDto hypothesis,
        PtnLocation location)
    {
        var code = NormalizeHypothesis(hypothesis.HypothesisKindCode, ApiHypothesisMap);
        var findingRef = CreateFindingRef(PtnSourceCheckerCodes.ApiContract, code, location, hypothesis.Evidence);
        return new PtnDiagnosisReport.PtnDiagnosisHypothesis
        {
            HypothesisCode = code,
            Priority = hypothesis.Priority,
            ConfidenceCode = hypothesis.ConfidenceCode,
            Title = hypothesis.Title,
            Detail = hypothesis.Detail,
            Ref = findingRef,
            Evidence = hypothesis.Evidence.Select(item => new PtnEvidence
            {
                ProbeKindCode = item.ProbeKindCode,
                FactCode = NormalizeFact(item.FactCode, ApiFactMap),
                ExpectedValue = item.ExpectedValue,
                ObservedValue = item.ObservedValue,
                ObservedAtMs = item.ObservedAtMs,
                Ref = findingRef
            }).ToList(),
            NextChecks = hypothesis.NextChecks
        };
    }

    // Database hipotezini tek gramer, kanit olgulari ve kaynakli fingerprint ile cevirir.
    private static PtnDiagnosisReport.PtnDiagnosisHypothesis MapDatabaseHypothesis(
        DatabaseDiagnosisReportDto.HypothesisDto hypothesis,
        PtnLocation location)
    {
        var code = NormalizeHypothesis(hypothesis.HypothesisKindCode, DatabaseHypothesisMap);
        var findingRef = CreateFindingRef(PtnSourceCheckerCodes.DatabaseComparison, code, location, hypothesis.Evidence);
        return new PtnDiagnosisReport.PtnDiagnosisHypothesis
        {
            HypothesisCode = code,
            Priority = hypothesis.Priority,
            ConfidenceCode = hypothesis.ConfidenceCode,
            Title = hypothesis.Title,
            Detail = hypothesis.Detail,
            Ref = findingRef,
            Evidence = hypothesis.Evidence.Select(item => new PtnEvidence
            {
                ProbeKindCode = item.ProbeKindCode,
                FactCode = NormalizeFact(item.FactCode, DatabaseFactMap),
                ExpectedValue = item.ExpectedValue,
                ObservedValue = item.ObservedValue,
                Ref = findingRef
            }).ToList(),
            NextChecks = hypothesis.NextChecks
        };
    }

    // Ortak database sinyalini assertion veya provider-exception DTO'sundan tam birine cevirir.
    private static DatabaseDiagnoseRequestDto MapDatabaseRequest(PtnDiagnosisRequest request)
    {
        var result = new DatabaseDiagnoseRequestDto { ConnectionId = request.ConnectionId };
        if (!string.IsNullOrWhiteSpace(request.OutcomeCode))
        {
            result.Assertion = new DatabaseDiagnoseRequestDto.AssertionSignalDto
            {
                SchemaName = request.Location.DbSchemaName ?? string.Empty,
                TableName = request.Location.DbTableName ?? string.Empty,
                KeyValues = request.KeyValues,
                OutcomeCode = ToDatabaseOutcome(request.OutcomeCode),
                FailedExpectations = request.FailedExpectations.Select(MapFailedExpectation).ToList()
            };
            return result;
        }

        result.DbException = new DatabaseDiagnoseRequestDto.DatabaseExceptionSignalDto
        {
            EngineCode = request.EngineCode ?? string.Empty,
            SqlState = request.SqlState ?? string.Empty,
            ProviderFields = request.ProviderFields
        };
        return result;
    }

    // Redaksiyonlu kopru failure modelini database checker teshis DTO'suna cevirir.
    private static FailedExpectationDto MapFailedExpectation(
        PtnAssertionResult.PtnFailedExpectation expectation)
    {
        return new FailedExpectationDto
        {
            ColumnName = expectation.ColumnName,
            MatcherKindCode = expectation.MatcherKindCode,
            ExpectedValue = expectation.ExpectedValue,
            ObservedValue = expectation.ObservedValue
        };
    }

    // Kaynak checker, hipotez, konum ve kanit kimliginden kaynak-ayrik SHA-256 fingerprint olusturur.
    private static PtnFindingRef CreateFindingRef(
        string sourceCheckerCode,
        string hypothesisCode,
        PtnLocation location,
        object evidence)
    {
        var canonical = JsonSerializer.Serialize(new { sourceCheckerCode, hypothesisCode, location, evidence });
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        return new PtnFindingRef
        {
            SourceCheckerCode = sourceCheckerCode,
            Fingerprint = PtnBridgeSettingNames.FingerprintPrefix + hash
        };
    }

    // Snapshot kimligi olmayan API teshis istegini checker cagrisi yerine kararli hata yapar.
    private static Guid RequireSnapshotId(Guid? snapshotId)
    {
        return snapshotId ?? throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);
    }

    // Kaynak hipotez kodunu tek kopru gramerine cevirir; bilinmeyen drift'i hata yapar.
    private static string NormalizeHypothesis(
        string code,
        IReadOnlyDictionary<string, string> mapping)
    {
        return mapping.TryGetValue(code, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);
    }

    // Kaynak probe olgusunu tek kopru sozlugune cevirir; bilinmeyen drift'i hata yapar.
    private static string NormalizeFact(string code, IReadOnlyDictionary<string, string> mapping)
    {
        return mapping.TryGetValue(code, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed);
    }

    // Normalize outcome'u API checker'in kaynak casing ve gramerine geri cevirir.
    private static string? ToApiOutcome(string? outcomeCode)
    {
        return outcomeCode is null
            ? null
            : ApiOutcomeMap.Single(pair => pair.Value == outcomeCode).Key;
    }

    // Normalize outcome'u database checker'in kaynak casing ve gramerine geri cevirir.
    private static string ToDatabaseOutcome(string outcomeCode)
    {
        return DatabaseOutcomeMap.Single(pair => pair.Value == outcomeCode).Key;
    }

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
