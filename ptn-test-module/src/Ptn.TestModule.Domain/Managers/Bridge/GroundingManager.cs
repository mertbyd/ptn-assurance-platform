using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Footprint;
using Ptn.TestModule.Models.Bridge.Database;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Profil kapsami ve ortam yetenegini tek ptn_ground sonucunda birlestirir.
// sistemdeki gorevi: Cozulemeyen operasyon referansini aday listesi veya tahmin yerine kapali soruya cevirir.
public class GroundingManager : TestModuleDomainService
{
    private readonly ProfilePackManager _profilePackManager;
    private readonly FootprintCapabilityManager _footprintCapabilityManager;

    public GroundingManager(
        ProfilePackManager profilePackManager,
        FootprintCapabilityManager footprintCapabilityManager)
    {
        _profilePackManager = profilePackManager;
        _footprintCapabilityManager = footprintCapabilityManager;
    }
    // Profil, kapsam ve footprint yetenegini tek ozet-once grounding sonucunda toplar.
    public GroundingResult Ground(
        GroundRequest request,
        ProfilePack pack,
        string currentFingerprint,
        CapabilityLevel capability)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        var coverage = _profilePackManager.BuildCoverage(pack, PtnConceptCodes.All);
        var result = new GroundingResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = coverage,
            DecisionCode = PtnVerdictCodes.Inconclusive,
            CriticalFactCode = TestModuleBridgeErrorCodes.EvidenceUnavailable,
            Footprint = _footprintCapabilityManager.Describe(capability),
            Questions = [OperationQuestion(request.OperationReferenceId)],
            ResourceLink = ResourceLink(request.ResponseFormat, PtnToolCodes.Ground)
        };
        ApplyOperationThreshold(result);
        return result;
    }
    // Yayin kapisini referans cozulemediginde kapali ve ogreten bir sonuc olarak kapatir.
    public ValidationResult Validate(
        ValidateRequest request,
        ProfilePack pack,
        string currentFingerprint,
        DatabaseDerivabilityResult? databaseDerivability = null)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        return new ValidationResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = _profilePackManager.BuildCoverage(pack, PtnConceptCodes.All),
            IsPublishable = false,
            DecisionCode = PtnVerdictCodes.Inconclusive,
            DatabaseDerivability = databaseDerivability,
            Questions = [AssertionQuestion(request.AssertionReferenceIds)],
            ResourceLink = ResourceLink(request.ResponseFormat, PtnToolCodes.Validate)
        };
    }
    // Validate domain girdisinden DB checker turetilebilirlik istegini tek yerde kurar.
    public DatabaseDerivabilityRequest CreateDatabaseDerivabilityRequest(ValidateRequest request) => new()
    {
        ConnectionId = request.ConnectionId,
        Assertions = request.DatabaseAssertions.ToList()
    };
    // Cozulemeyen operasyon referansini tek kapali onay sorusuna cevirir.
    private static ClosedQuestion OperationQuestion(Guid operationReferenceId) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = [operationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)],
        GapKindCode = PtnOpenQuestionCodes.OperationReferenceRequired
    };
    // Assertion referanslarini serbest JSON pointer tahmini yerine kapali secenek olarak korur.
    private static ClosedQuestion AssertionQuestion(IEnumerable<Guid> assertionReferenceIds) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.AssertionReferenceRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = assertionReferenceIds
            .Select(id => id.ToString(PtnBridgeConsts.ReferenceIdFormat))
            .ToList(),
        GapKindCode = PtnOpenQuestionCodes.AssertionReferenceRequired
    };
    // Esik alti operasyon adaylarini listeden cikarip kapali secim sorusuna cevirir.
    private static void ApplyOperationThreshold(GroundingResult result)
    {
        if (result.OperationBinding is null)
        {
            return;
        }
        var confident = result.OperationBinding.Suggestions
            .Where(item => item.Score >= PtnBridgeConsts.MinimumOperationScore)
            .ToList();
        if (confident.Count > 0)
        {
            result.OperationBinding.Suggestions = confident;
            return;
        }
        var options = result.OperationBinding.Suggestions
            .Select(item => item.SourceOperationId ?? string.Join(
                PtnBridgeConsts.EvidenceReferenceSeparator,
                item.SourceMethod,
                item.SourcePath))
            .ToList();
        result.OperationBinding.Suggestions = [];
        result.Questions.Add(OperationSelectionQuestion(options));
    }
    // Esik alti aday kimliklerini tek kapali secim sorusunda tasir.
    private static ClosedQuestion OperationSelectionQuestion(List<string> options) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.OperationSelectionRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = options,
        GapKindCode = PtnOpenQuestionCodes.OperationSelectionRequired
    };
    // Concise cevapta agir govdeyi tool resource adresine tasir.
    private static string? ResourceLink(string responseFormat, string toolCode) =>
        responseFormat == PtnResponseFormatCodes.Concise ? PtnBridgeRoutes.Resource(toolCode) : null;
}
