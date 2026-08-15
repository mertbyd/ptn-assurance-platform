using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Volo.Abp;
using CheckerOperationLinkSourceCodes = Ptn.ApiContractChecker.Constants.Conformance.Lookups.OperationLinkSourceCodes;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: API checker sonuc kodlarini tek Bridge sozlugune normalize eder.
// sistemdeki gorevi: Application servisini outcome siniflandirma kararindan uzak tutar.
public class ApiOracleManager : TestModuleDomainService
{
    // Operasyon sorgusunu checker'in kapali verbosity koduyla tamamlar.
    public ApiOperationRequest CreateOperationRequest(
        OperationQuery query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ApiOperationRequest
        {
            SnapshotId = query.SnapshotId,
            OperationId = query.OperationId,
            Method = query.Method,
            Path = query.Path,
            VerbosityCode = PtnApiOracleRequestCodes.MinimalVerbosity
        };
    }

    // Operation link istegini checker cagrisi oncesinde iptal kapisindan gecirir.
    public OperationLinkRequest PrepareOperationLinkRequest(
        OperationLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return request;
    }

    // Response gozlemini senaryonun sectigi veya geriye uyumlu varsayilan uygunluk profiliyle tamamlar.
    public ApiResponseRequest CreateResponseRequest(
        ResponseObservation observation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ApiResponseRequest
        {
            SnapshotId = observation.SnapshotId,
            OperationId = observation.OperationId,
            Method = observation.Method,
            Path = observation.Path,
            StatusCode = observation.StatusCode,
            ContentType = observation.ContentType,
            Headers = observation.Headers,
            Body = observation.Body,
            ProfileCode = ResolveConformanceProfile(observation.ProfileCode),
            Correlation = observation.Correlation
        };
    }

    // Turetilebilirlik istegini dis checker cagrisi oncesinde iptal kapisindan gecirir.
    public DerivabilityRequest PrepareDerivabilityRequest(
        DerivabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return request;
    }

    // Operasyon baglama sonucunun outcome kodunu kanoniklestirir.
    public OperationBinding Normalize(OperationBinding result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Opsiyonel senaryo profilini kapali sozlukten cozer; secim yoksa Runtime uygular.
    private static string ResolveConformanceProfile(string? profileCode)
    {
        if (string.IsNullOrWhiteSpace(profileCode))
        {
            return PtnConformanceProfileCodes.Runtime;
        }

        return PtnConformanceProfileCodes.All.Contains(profileCode)
            ? profileCode
            : throw new BusinessException(TestModuleBridgeErrorCodes.Validation.ProfileCodeInvalid)
                .WithData(nameof(profileCode), profileCode);
    }

    // Operation link sonucunu tek outcome ve kaynak sozlugune normalize eder.
    public OperationLinkResult Normalize(OperationLinkResult result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        result.Candidates.ForEach(candidate =>
            candidate.SourceCode = NormalizeOperationLinkSource(candidate.SourceCode));
        return result;
    }

    // Request ornegi sonucunun outcome kodunu kanoniklestirir.
    public RequestExample Normalize(RequestExample result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Turetilebilirlik listesindeki her outcome kodunu kanoniklestirir.
    public DerivabilityResult Normalize(DerivabilityResult result)
    {
        result.Assertions.ForEach(assertion => assertion.OutcomeCode = NormalizeOutcome(assertion.OutcomeCode));
        return result;
    }

    // Response uygunluk sonucunun outcome kodunu kanoniklestirir.
    public ConformanceResult Normalize(
        ResponseObservation request,
        ConformanceResult result)
    {
        if (!CorrelationMatches(request.Correlation, result.Correlation))
        {
            return new ConformanceResult
            {
                OutcomeCode = PtnOutcomeCodes.Unavailable,
                Correlation = request.Correlation
            };
        }

        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Istenen ve echo edilen API korelasyon alanlarini ordinal olarak karsilastirir.
    private static bool CorrelationMatches(CorrelationRef? expected, CorrelationRef? actual) =>
        expected?.TraceId == actual?.TraceId && expected?.StepKey == actual?.StepKey;

    // Checker kodunu Bridge'in tek outcome sozlugune cevirir.
    private static string NormalizeOutcome(string outcomeCode) =>
        OutcomeMap.TryGetValue(outcomeCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed)
                .WithData(nameof(outcomeCode), outcomeCode);

    // Checker operation link kaynagini Bridge'in kapali kaynak sozlugune cevirir.
    private static string NormalizeOperationLinkSource(string sourceCode) =>
        OperationLinkSourceMap.TryGetValue(sourceCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed)
                .WithData(nameof(sourceCode), sourceCode);

    private static readonly IReadOnlyDictionary<string, string> OutcomeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConformanceOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [ConformanceOutcomeCodes.StatusCodeUndocumented] = PtnOutcomeCodes.StatusCodeUndocumented,
            [ConformanceOutcomeCodes.MediaTypeUndocumented] = PtnOutcomeCodes.MediaTypeUndocumented,
            [ConformanceOutcomeCodes.ResponseSchemaViolation] = PtnOutcomeCodes.ResponseSchemaViolation,
            [ConformanceOutcomeCodes.RequiredHeaderMissing] = PtnOutcomeCodes.RequiredHeaderMissing,
            [ConformanceOutcomeCodes.UndocumentedProperty] = PtnOutcomeCodes.UndocumentedProperty,
            [ConformanceOutcomeCodes.ServerError] = PtnOutcomeCodes.ServerError,
            [ConformanceOutcomeCodes.OperationNotResolved] = PtnOutcomeCodes.OperationNotResolved,
            [ConformanceOutcomeCodes.SnapshotNotFound] = PtnOutcomeCodes.SnapshotNotFound,
            [ConformanceOutcomeCodes.PolicySuppressed] = PtnOutcomeCodes.PolicySuppressed,
            [ConformanceOutcomeCodes.SchemaNotResolved] = PtnOutcomeCodes.SchemaNotResolved,
            [AssertionDerivabilityCodes.Derivable] = PtnOutcomeCodes.Derivable,
            [AssertionDerivabilityCodes.AssertionNotInContract] = PtnOutcomeCodes.AssertionNotInContract,
            [AssertionDerivabilityCodes.DerivableButOptional] = PtnOutcomeCodes.DerivableButOptional
        };

    private static readonly IReadOnlyDictionary<string, string> OperationLinkSourceMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CheckerOperationLinkSourceCodes.DeclaredLink] = PtnOperationLinkSourceCodes.DeclaredLink,
            [CheckerOperationLinkSourceCodes.SchemaMatch] = PtnOperationLinkSourceCodes.SchemaMatch,
            [CheckerOperationLinkSourceCodes.LocationHeader] = PtnOperationLinkSourceCodes.LocationHeader
        };
}
