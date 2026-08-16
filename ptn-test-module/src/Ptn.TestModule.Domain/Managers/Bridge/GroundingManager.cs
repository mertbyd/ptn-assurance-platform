using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Footprint;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Profil kapsami, grounding kaniti ve mevcut yayin gate kararini Bridge sonucunda birlestirir.
// sistemdeki gorevi: Cozulemeyen yazarlik kanitini tahmin yerine kapali soruya cevirir.
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
        CapabilityLevel capability,
        SnapshotOperationInventory inventory,
        SnapshotOperation? operation,
        RequestExample? requestExample,
        ConceptBinding? tableBinding,
        TableDescription? tableDescription)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        var coverage = _profilePackManager.BuildCoverage(pack, PtnConceptCodes.All);
        var questions = BuildGroundingQuestions(request, pack, inventory, operation, tableBinding);
        var isGrounded = operation is not null &&
                         requestExample?.OutcomeCode == PtnOutcomeCodes.Passed &&
                         tableBinding is not null &&
                         tableDescription is not null;
        var result = new GroundingResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = coverage,
            DecisionCode = isGrounded ? PtnVerdictCodes.Confirmed : PtnVerdictCodes.Inconclusive,
            CriticalFactCode = isGrounded ? PtnOutcomeCodes.Passed : TestModuleBridgeErrorCodes.EvidenceUnavailable,
            OperationBinding = BuildOperationBinding(request, inventory, operation),
            RequestExample = requestExample,
            TableDescription = tableDescription,
            Footprint = _footprintCapabilityManager.Describe(capability),
            Questions = questions,
            ResourceLink = ResourceLink(request.ResponseFormat, PtnToolCodes.Ground)
        };
        return result;
    }
    // Kapali referans varsa onu, yoksa esik ustu tek leksik adayi secer; envanter disina cikmaz.
    public SnapshotOperation? ResolveOperation(
        GroundRequest request,
        SnapshotOperationInventory inventory)
    {
        if (!inventory.IsComplete || inventory.OutcomeCode != PtnOutcomeCodes.Passed)
        {
            return null;
        }
        if (request.OperationReferenceId.HasValue)
        {
            return inventory.Items.FirstOrDefault(item => item.ReferenceId == request.OperationReferenceId.Value);
        }
        var candidates = inventory.Items
            .Select(item => new { Item = item, Score = ScoreOperation(request.StepIntent, item) })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Item.OperationId)
            .ThenBy(item => item.Item.Method)
            .ThenBy(item => item.Item.Path)
            .ToList();
        return candidates.Count > 0 &&
               candidates[0].Score >= PtnBridgeConsts.MinimumOperationScore &&
               (candidates.Count == 1 || candidates[0].Score > candidates[1].Score)
            ? candidates[0].Item
            : null;
    }
    // Kapali tablo referansini onayli profil baglarinda cozer; tek bag varsa mekanik olarak secer.
    public ConceptBinding? ResolveTableBinding(
        GroundRequest request,
        ProfilePack pack,
        string currentFingerprint)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        var approved = pack.Bindings
            .Where(item => item.StateCode == PtnBindingStateCodes.Approved)
            .OrderBy(item => item.ConceptCode)
            .ToList();
        if (request.TableReferenceId.HasValue)
        {
            return approved.FirstOrDefault(item =>
                CreateTableReferenceId(request.ProfileKey, item) == request.TableReferenceId.Value);
        }
        return approved.Count == 1 ? approved[0] : null;
    }
    // Secilen gercek envanter satirini checker request-example sorgusuna cevirir.
    public OperationQuery CreateOperationQuery(GroundRequest request, SnapshotOperation operation) => new()
    {
        SnapshotId = request.SpecSnapshotId,
        OperationId = operation.OperationId,
        Method = operation.Method,
        Path = operation.Path
    };
    // Onayli profil bagini DB checker tablo sorgusuna cevirir.
    public TableQuery CreateTableQuery(GroundRequest request, ConceptBinding binding) => new()
    {
        ConnectionId = request.ConnectionId,
        DbSchemaName = binding.DbSchemaName,
        TableName = binding.TableName
    };
    // Validate girdisinde derlenebilir kaynak kaniti varsa ortak yayin adayi kurar.
    public ScenarioPublicationCandidate? CreatePublicationCandidate(ValidateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.SourceDocument) || request.MaterialSeal is null)
        {
            return null;
        }
        return new ScenarioPublicationCandidate
        {
            SourceDocument = request.SourceDocument,
            RulesFingerprint = request.MaterialSeal.RulesFingerprint,
            SpecSnapshotId = request.MaterialSeal.SpecSnapshotId,
            SpecFingerprint = request.MaterialSeal.SpecFingerprint,
            DbConnectionId = request.MaterialSeal.DbConnectionId,
            DbSchemaFingerprint = request.MaterialSeal.DbSchemaFingerprint,
            ProfileFingerprint = request.MaterialSeal.ProfileFingerprint
        };
    }
    // Mevcut yayin gate kararini Bridge'in ozet-once ve aciklanabilir sonucuna cevirir.
    public ValidationResult Validate(
        ValidateRequest request,
        ProfilePack pack,
        string currentFingerprint,
        ScenarioCompilationEvidence? evidence,
        TestScenarioPublishDecision? decision)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        var hasEvidence = evidence is not null && decision is not null;
        return new ValidationResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = _profilePackManager.BuildCoverage(pack, PtnConceptCodes.All),
            IsPublishable = decision?.IsPublishable ?? false,
            DecisionCode = !hasEvidence
                ? PtnVerdictCodes.Inconclusive
                : decision!.IsPublishable ? PtnVerdictCodes.Confirmed : PtnVerdictCodes.RuledOut,
            IsSchemaValid = evidence?.IsSchemaValid ?? false,
            AssertionCount = evidence?.AssertionCount ?? 0,
            Derivability = evidence?.ApiDerivability,
            DatabaseDerivability = evidence?.DatabaseDerivability,
            FailedGateCodes = decision?.FailedGateCodes ?? [],
            Warnings = decision?.Warnings ?? [],
            LintDiagnostics = evidence?.LintDiagnostics ?? string.Empty,
            Questions = hasEvidence ? [] : [AssertionQuestion(request.AssertionReferenceIds)],
            ResourceLink = ResourceLink(request.ResponseFormat, PtnToolCodes.Validate)
        };
    }
    // Cozulemeyen operasyon referansini tek kapali onay sorusuna cevirir.
    private static ClosedQuestion OperationQuestion(IEnumerable<Guid> operationReferenceIds) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = operationReferenceIds.Select(id => id.ToString(PtnBridgeConsts.ReferenceIdFormat)).ToList(),
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
    // Grounding eksiklerini gercek operasyon ve onayli tablo referanslarindan kapali sorulara cevirir.
    private static List<ClosedQuestion> BuildGroundingQuestions(
        GroundRequest request,
        ProfilePack pack,
        SnapshotOperationInventory inventory,
        SnapshotOperation? operation,
        ConceptBinding? tableBinding)
    {
        var questions = new List<ClosedQuestion>();
        if (operation is null)
        {
            var operationOptions = inventory.IsComplete
                ? inventory.Items.Select(item => item.ReferenceId)
                : [];
            questions.Add(OperationQuestion(operationOptions));
        }
        if (operation is not null && tableBinding is null)
        {
            questions.Add(TableSelectionQuestion(pack.Bindings
                .Where(item => item.StateCode == PtnBindingStateCodes.Approved)
                .Select(item => CreateTableReferenceId(request.ProfileKey, item))));
        }
        return questions;
    }
    // Envanterdeki gercek adaylari mekanik skor ve adresleriyle operation binding sonucuna tasir.
    private static OperationBinding BuildOperationBinding(
        GroundRequest request,
        SnapshotOperationInventory inventory,
        SnapshotOperation? selected)
    {
        var items = selected is null && inventory.IsComplete ? inventory.Items : selected is null ? [] : [selected];
        return new OperationBinding
        {
            OutcomeCode = inventory.OutcomeCode,
            Suggestions = items.Select(item => new OperationSuggestion
            {
                ReferenceId = item.ReferenceId,
                SourceOperationId = item.OperationId,
                SourceMethod = item.Method,
                SourcePath = item.Path,
                Score = ScoreOperation(request.StepIntent, item)
            }).OrderByDescending(item => item.Score)
              .ThenBy(item => item.SourceOperationId)
              .ThenBy(item => item.SourceMethod)
              .ThenBy(item => item.SourcePath)
              .ToList()
        };
    }
    // Onayli profil tablo baglarini opak referanslarla tek kapali secim sorusuna cevirir.
    private static ClosedQuestion TableSelectionQuestion(IEnumerable<Guid> options) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.TableSelectionRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = options.Select(id => id.ToString(PtnBridgeConsts.ReferenceIdFormat)).ToList(),
        GapKindCode = PtnOpenQuestionCodes.TableSelectionRequired
    };
    // Niyet ve gercek operasyon adresindeki ortak kapali token oranini yuzdelik skora cevirir.
    private static int ScoreOperation(string stepIntent, SnapshotOperation operation)
    {
        var intentTokens = Tokens(stepIntent);
        if (intentTokens.Count == 0)
        {
            return 0;
        }
        var operationTokens = Tokens(string.Join(
            PtnBridgeConsts.EvidenceReferenceSeparator,
            operation.OperationId,
            operation.Method,
            operation.Path));
        return intentTokens.Intersect(operationTokens, StringComparer.Ordinal).Count() * 100 / intentTokens.Count;
    }
    // Leksik eslemeyi kulturden ve ayirac biciminden bagimsiz kararli tokenlara indirger.
    private static HashSet<string> Tokens(string? value) => Regex.Matches(
            value?.ToLowerInvariant() ?? string.Empty,
            "[a-z0-9]+",
            RegexOptions.CultureInvariant)
        .Select(match => match.Value)
        .ToHashSet(StringComparer.Ordinal);
    // Profil, kavram ve somut tablo adresinden kararli kapali referans uretir.
    private static Guid CreateTableReferenceId(string profileKey, ConceptBinding binding)
    {
        var canonical = string.Join(
            PtnBridgeConsts.EvidenceReferenceSeparator,
            profileKey,
            binding.ConceptCode,
            binding.DbSchemaName,
            binding.TableName);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return new Guid(bytes.AsSpan(0, 16));
    }
    // Concise cevapta agir govdeyi tool resource adresine tasir.
    private static string? ResourceLink(string responseFormat, string toolCode) =>
        responseFormat == PtnResponseFormatCodes.Concise ? PtnBridgeRoutes.Resource(toolCode) : null;
}
