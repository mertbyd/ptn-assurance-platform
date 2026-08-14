using System;
using System.Collections.Generic;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: API checker sonuc kodlarini tek Bridge sozlugune normalize eder.
// sistemdeki gorevi: Application servisini outcome siniflandirma kararindan uzak tutar.
public class ApiOracleManager : TestModuleDomainService
{
    // Operasyon sorgusunu checker'in kapali verbosity koduyla tamamlar.
    public PtnApiOperationRequest CreateOperationRequest(PtnOperationQuery query) =>
        new()
        {
            SnapshotId = query.SnapshotId,
            OperationId = query.OperationId,
            Method = query.Method,
            Path = query.Path,
            VerbosityCode = PtnApiOracleRequestCodes.MinimalVerbosity
        };

    // Response gozlemini checker'in runtime profil koduyla tamamlar.
    public PtnApiResponseRequest CreateResponseRequest(PtnResponseObservation observation) =>
        new()
        {
            SnapshotId = observation.SnapshotId,
            OperationId = observation.OperationId,
            Method = observation.Method,
            Path = observation.Path,
            StatusCode = observation.StatusCode,
            ContentType = observation.ContentType,
            Headers = observation.Headers,
            Body = observation.Body,
            ProfileCode = PtnApiOracleRequestCodes.RuntimeProfile
        };

    // Operasyon baglama sonucunun outcome kodunu kanoniklestirir.
    public PtnOperationBinding Normalize(PtnOperationBinding result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Request ornegi sonucunun outcome kodunu kanoniklestirir.
    public PtnRequestExample Normalize(PtnRequestExample result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Turetilebilirlik listesindeki her outcome kodunu kanoniklestirir.
    public PtnDerivabilityResult Normalize(PtnDerivabilityResult result)
    {
        result.Assertions.ForEach(assertion => assertion.OutcomeCode = NormalizeOutcome(assertion.OutcomeCode));
        return result;
    }

    // Response uygunluk sonucunun outcome kodunu kanoniklestirir.
    public PtnConformanceResult Normalize(PtnConformanceResult result)
    {
        result.OutcomeCode = NormalizeOutcome(result.OutcomeCode);
        return result;
    }

    // Checker kodunu Bridge'in tek outcome sozlugune cevirir.
    private static string NormalizeOutcome(string outcomeCode) =>
        OutcomeMap.TryGetValue(outcomeCode, out var normalized)
            ? normalized
            : throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed)
                .WithData(nameof(outcomeCode), outcomeCode);

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
}
