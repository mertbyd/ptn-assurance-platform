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
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Profil verisindeki kanit yoluyla sirali gozlemleri birlestirir ve mekanik hukum verir.
// sistemdeki gorevi: Vaka-ozel if akislari veya ikinci bir akil yurutme motoru olmadan aciklama agaci uretir.
public class EvidenceChainManager : TestModuleDomainService
{
    private readonly ProfilePackManager _profilePackManager;

    // Profil karar sahibini kanit zincirine baglar.
    public EvidenceChainManager(ProfilePackManager profilePackManager)
    {
        _profilePackManager = profilePackManager;
    }

    // Application sinirinda toplanmis kanit dugumlerini profil yoluna gore mekanik hukumle birlestirir.
    public ChainResult Run(
        ProfilePack pack,
        AccessTuple tuple,
        IReadOnlyCollection<ExplanationNode> observations)
    {
        var path = SelectPath(pack, tuple);
        var coverage = _profilePackManager.BuildCoverage(pack, GetRequiredConcepts(path));
        if (coverage.UnboundConcepts.Count > 0)
        {
            return BuildUnboundResult(path, coverage);
        }

        if (ExceedsBudget(path))
        {
            return BuildBudgetResult(path, coverage);
        }

        var nodes = DropUnsupportedNodes(observations);
        return BuildResult(path, coverage, nodes);
    }

    // Kapali operasyon referansi cozulemediginde tahmin yerine Inconclusive aciklama sonucu dondurur.
    public ExplainResult Explain(
        ExplainRequest request,
        ProfilePack pack,
        string currentFingerprint)
    {
        _profilePackManager.GetValidated(pack, request.ProfileKey, currentFingerprint);
        return new ExplainResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = _profilePackManager.BuildCoverage(pack, PtnConceptCodes.All),
            VerdictCode = PtnVerdictCodes.Inconclusive,
            CriticalFactCode = TestModuleBridgeErrorCodes.EvidenceUnavailable,
            Questions = [CreateOperationQuestion(request.OperationReferenceId)],
            ResourceLink = request.ResponseFormat == PtnResponseFormatCodes.Concise
                ? PtnBridgeRoutes.Resource(PtnToolCodes.Explain)
                : null
        };
    }

    // Cozulemeyen operasyon referansini kapali onay secenegine cevirir.
    private static ClosedQuestion CreateOperationQuestion(Guid operationReferenceId) => new()
    {
        QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
        Prompt = TestModuleBridgeErrorCodes.EvidenceUnavailable,
        Options = [operationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)],
        GapKindCode = PtnOpenQuestionCodes.OperationReferenceRequired
    };

    // Status veya operasyon tetikleyicisine uyan tek profil yolunu secer.
    private static EvidencePathDefinition SelectPath(ProfilePack pack, AccessTuple tuple)
    {
        var path = pack.Paths.FirstOrDefault(item =>
            tuple.StatusCode is not null && item.Trigger.StatusCodes.Contains(tuple.StatusCode.Value) ||
            item.Trigger.OperationIds.Contains(tuple.OperationId));
        return path ?? throw new BusinessException(TestModuleBridgeErrorCodes.EvidencePathNotFound);
    }

    // Yol adimlarindaki kavram kodlarini sirali ve tekrarsiz kapsam girdisine cevirir.
    private static IReadOnlyCollection<string> GetRequiredConcepts(EvidencePathDefinition path)
    {
        return path.Steps
            .Where(step => step.ConceptCode is not null)
            .Select(step => step.ConceptCode!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    // Baglanmamis kavramlari kapali NOT_BOUND sorulariyla Inconclusive sonucuna cevirir.
    private static ChainResult BuildUnboundResult(
        EvidencePathDefinition path,
        CoverageReport coverage)
    {
        return new ChainResult
        {
            PathKey = path.PathKey,
            VerdictCode = PtnVerdictCodes.Inconclusive,
            Coverage = coverage,
            OpenQuestions = coverage.UnboundConcepts
                .Select(concept => PtnOpenQuestionCodes.ConceptNotBoundPrefix + concept)
                .ToList()
        };
    }

    // Hop veya dugum butcesini asan yolu probe calistirmadan Inconclusive yapar.
    private static ChainResult BuildBudgetResult(
        EvidencePathDefinition path,
        CoverageReport coverage)
    {
        return new ChainResult
        {
            PathKey = path.PathKey,
            VerdictCode = PtnVerdictCodes.Inconclusive,
            Coverage = coverage,
            HopCount = path.Steps.Count,
            BudgetExceeded = true
        };
    }

    // Step concept kodunu onayli profil baglamasina cozer.
    private ConceptBinding ResolveStepBinding(EvidenceStepExecutionContext context)
    {
        return _profilePackManager.ResolveConcept(
            context.Pack,
            context.Step.ConceptCode ?? throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid));
    }

    // Kavramin sonraki join veya hukum icin anlamli kolon degerlerini projeksiyon satirlarindan secer.
    private static List<string> ExtractProjectionValues(
        string? conceptCode,
        ConceptBinding binding,
        IEnumerable<Dictionary<string, string?>> rows)
    {
        var semanticRole = conceptCode == PtnConceptCodes.Subject
            ? PtnBindingColumnCodes.Identity
            : conceptCode == PtnConceptCodes.RoleAssignment
                ? PtnBindingColumnCodes.Role
                : PtnBindingColumnCodes.Permission;
        if (!binding.ColumnMap.TryGetValue(semanticRole, out var columnName))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        return rows.Select(row => row.GetValueOrDefault(columnName))
            .Where(value => value is not null)
            .Cast<string>()
            .ToList();
    }

    // Profil kolon rollerinden onceki dugum degerine bagli, SQL icermeyen projeksiyon istegi kurar.
    private static ProjectionRequest CreateProjectionRequest(
        EvidenceStepExecutionContext context,
        ConceptBinding binding)
    {
        return new ProjectionRequest
        {
            ConnectionId = context.Tuple.ConnectionId,
            DbSchemaName = binding.DbSchemaName,
            TableName = binding.TableName,
            KeyValues = CreateProjectionKeys(context, binding),
            ProjectColumns = binding.ColumnMap.Values.Distinct(StringComparer.Ordinal).ToList(),
            MaxRows = PtnBridgeConsts.MaxProjectionRows
        };
    }

    // Ilk kavramda subject referansini, sonraki kavramlarda joinFrom dugum kanitini anahtara baglar.
    private static Dictionary<string, string?> CreateProjectionKeys(
        EvidenceStepExecutionContext context,
        ConceptBinding binding)
    {
        var semanticKey = context.Step.JoinFromNodeKindCode is null
            ? PtnBindingColumnCodes.Identity
            : JoinSemanticKey(context.Step.ConceptCode);
        if (!binding.ColumnMap.TryGetValue(semanticKey, out var columnName))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        var value = context.Step.JoinFromNodeKindCode is null
            ? context.Tuple.SubjectRef
            : FindJoinValue(context);
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { [columnName] = value };
    }

    // Hedef kavram icin onceki dugumden tasinacak semantik kolon rolunu belirler.
    private static string JoinSemanticKey(string? conceptCode)
    {
        return conceptCode == PtnConceptCodes.RoleAssignment
            ? PtnBindingColumnCodes.Subject
            : PtnBindingColumnCodes.Role;
    }

    // joinFrom ile adlandirilan onceki dugumun ilk gozlenen kanit degerini dondurur.
    private static string? FindJoinValue(EvidenceStepExecutionContext context)
    {
        return context.Nodes
            .LastOrDefault(node => node.NodeKindCode == context.Step.JoinFromNodeKindCode)?
            .Evidence.Select(item => item.ObservedValue).FirstOrDefault(value => value is not null);
    }

    // Kaniti olmayan dugumleri rapor listesinden mekanik olarak dusurur.
    private static List<ExplanationNode> DropUnsupportedNodes(IEnumerable<ExplanationNode> nodes)
    {
        return nodes.Where(node => node.Evidence.Count > 0).ToList();
    }

    // Dugum listesini sirali agaca baglar ve unavailable/ifade kurallarindan hukum uretir.
    private static ChainResult BuildResult(
        EvidencePathDefinition path,
        CoverageReport coverage,
        List<ExplanationNode> nodes)
    {
        LinkAsChain(nodes);
        return new ChainResult
        {
            PathKey = path.PathKey,
            VerdictCode = EvaluateVerdict(path, nodes),
            Root = nodes.FirstOrDefault(),
            Coverage = coverage,
            HopCount = nodes.Count
        };
    }

    // Sirali dugum listesini her dugumun tek cocugu olacak bicimde aciklama agacina cevirir.
    private static void LinkAsChain(IReadOnlyList<ExplanationNode> nodes)
    {
        for (var index = 0; index + 1 < nodes.Count; index++)
        {
            nodes[index].Children = [nodes[index + 1]];
        }
    }

    // Unavailable'i Inconclusive yapar; kalan durumda kapali confirmed ifadesini mekanik degerlendirir.
    private static string EvaluateVerdict(
        EvidencePathDefinition path,
        IReadOnlyCollection<ExplanationNode> nodes)
    {
        if (nodes.Any(node => node.StateCode == PtnEvidenceStateCodes.Unavailable))
        {
            return PtnVerdictCodes.Inconclusive;
        }

        return EvaluateConfirmed(path.ConfirmedWhen, nodes)
            ? PtnVerdictCodes.Confirmed
            : PtnVerdictCodes.RuledOut;
    }

    // AND ile baglanan kapali observed/containsAny atomlarinin tamamini degerlendirir.
    private static bool EvaluateConfirmed(
        string expression,
        IReadOnlyCollection<ExplanationNode> nodes)
    {
        return expression.Split(
                PtnEvidenceExpressionPatterns.AndSeparator,
                StringSplitOptions.RemoveEmptyEntries)
            .All(atom => EvaluateAtom(atom, nodes));
    }

    // Tek observed veya containsAny atomunu dugum durum ve kanit degerlerine karsi cozer.
    private static bool EvaluateAtom(string atom, IReadOnlyCollection<ExplanationNode> nodes)
    {
        if (atom.EndsWith(PtnEvidenceExpressionTokens.ObservedSuffix, StringComparison.Ordinal))
        {
            var nodeKind = atom[..^PtnEvidenceExpressionTokens.ObservedSuffix.Length];
            return nodes.Any(node =>
                node.NodeKindCode == nodeKind && node.StateCode == PtnEvidenceStateCodes.Observed);
        }

        return EvaluateContainsAny(atom, nodes);
    }

    // containsAny atomunun iki dugum deger kumesini ve istege bagli olumsuzlamasini degerlendirir.
    private static bool EvaluateContainsAny(
        string atom,
        IReadOnlyCollection<ExplanationNode> nodes)
    {
        var negated = atom.StartsWith(PtnEvidenceExpressionTokens.Negation, StringComparison.Ordinal);
        var normalized = negated ? atom[PtnEvidenceExpressionTokens.Negation.Length..] : atom;
        var separatorIndex = normalized.IndexOf(PtnEvidenceExpressionTokens.ContainsAny, StringComparison.Ordinal);
        var leftKind = normalized[..separatorIndex];
        var rightStart = separatorIndex + PtnEvidenceExpressionTokens.ContainsAny.Length;
        var rightKind = normalized[rightStart..^PtnEvidenceExpressionTokens.ValuesSuffix.Length];
        var intersects = ValuesFor(nodes, leftKind).Intersect(ValuesFor(nodes, rightKind)).Any();
        return negated ? !intersects : intersects;
    }

    // Adlandirilan dugum turundeki bos olmayan kanit degerlerini ordinal kume olarak dondurur.
    private static IReadOnlyCollection<string> ValuesFor(
        IEnumerable<ExplanationNode> nodes,
        string nodeKindCode)
    {
        return nodes.Where(node => node.NodeKindCode == nodeKindCode)
            .SelectMany(node => node.Evidence)
            .Select(item => item.ObservedValue)
            .Where(value => value is not null)
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    // Bir port cevabini state, alaka, konum ve butceli kanit listesiyle dugume cevirir.
    private static ExplanationNode CreateNode(
        EvidenceStepExecutionContext context,
        string stateCode,
        List<Evidence> evidence,
        Location location)
    {
        return new ExplanationNode
        {
            NodeKindCode = context.Step.NodeKindCode,
            StateCode = stateCode,
            RelevanceCode = IsHighRelevance(context.Step.NodeKindCode)
                ? PtnRelevanceCodes.High
                : PtnRelevanceCodes.Normal,
            Location = location,
            Evidence = evidence.Take(PtnBridgeConsts.MaxEvidencePerNode).ToList()
        };
    }

    // Unavailable port cevabini kanitli ve Inconclusive'a zorlayan dugume cevirir.
    private static ExplanationNode CreateUnavailableNode(
        EvidenceStepExecutionContext context,
        Location? location = null)
    {
        var evidence = new Evidence
        {
            ProbeKindCode = PtnProbeKindCodes.BridgeAvailability,
            FactCode = PtnFactCodes.Unavailable,
            Ref = CreateBridgeRef(context)
        };
        return CreateNode(
            context,
            PtnEvidenceStateCodes.Unavailable,
            [evidence],
            location ?? new Location());
    }

    // Porttan gelen her kapali degeri ayri ve kaynakli kanita cevirir; bos listeyi de Absent kanitiyla korur.
    private static List<Evidence> CreateValueEvidence(
        string probeKindCode,
        IReadOnlyCollection<string> values,
        FindingRef findingRef,
        string? expectedValue = null)
    {
        if (values.Count == 0)
        {
            return [new Evidence { ProbeKindCode = probeKindCode, FactCode = PtnFactCodes.Absent, Ref = findingRef }];
        }

        return values.Take(PtnBridgeConsts.MaxEvidencePerNode).Select(value => new Evidence
        {
            ProbeKindCode = probeKindCode,
            FactCode = PtnFactCodes.Present,
            ExpectedValue = expectedValue,
            ObservedValue = value,
            Ref = findingRef
        }).ToList();
    }

    // Yol ve dugum kimliginden Bridge kaynakli, kaynak-ayrik SHA-256 kanit referansi olusturur.
    private static FindingRef CreateBridgeRef(EvidenceStepExecutionContext context)
    {
        var canonical = string.Join(
            PtnBridgeConsts.EvidenceReferenceSeparator,
            context.Path.PathKey,
            context.Step.NodeKindCode,
            context.Step.SourceCode);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        return new FindingRef
        {
            SourceCheckerCode = PtnSourceCheckerCodes.Bridge,
            Fingerprint = PtnBridgeSettingNames.FingerprintPrefix + hash
        };
    }

    // Access tuple alanlarini iki checker icin ortak teshis port girdisine cevirir.
    private static DiagnosisRequest CreateDiagnosisRequest(AccessTuple tuple)
    {
        return new DiagnosisRequest
        {
            SpecSnapshotId = tuple.SpecSnapshotId,
            ConnectionId = tuple.ConnectionId,
            Location = CreateApiLocation(tuple),
            StatusCode = tuple.StatusCode
        };
    }

    // Access tuple operasyon adresini API oracle sorgusuna cevirir.
    private static OperationQuery CreateOperationQuery(AccessTuple tuple)
    {
        return new OperationQuery
        {
            SnapshotId = tuple.SpecSnapshotId!.Value,
            OperationId = tuple.OperationId,
            Method = tuple.Method,
            Path = tuple.Path
        };
    }

    // Access tuple ve kapali context pointer'larini turetilebilirlik istegine cevirir.
    private static DerivabilityRequest CreateDerivabilityRequest(AccessTuple tuple)
    {
        tuple.Context.TryGetValue(PtnBridgeContextKeys.AssertionPaths, out var pathsJson);
        tuple.Context.TryGetValue(PtnBridgeContextKeys.MediaType, out var mediaType);
        return new DerivabilityRequest
        {
            SnapshotId = tuple.SpecSnapshotId!.Value,
            OperationId = tuple.OperationId,
            Method = tuple.Method,
            Path = tuple.Path,
            StatusCode = tuple.StatusCode?.ToString(CultureInfo.InvariantCulture),
            MediaType = mediaType,
            AssertionPaths = string.IsNullOrWhiteSpace(pathsJson)
                ? []
                : JsonSerializer.Deserialize<List<string>>(pathsJson) ?? []
        };
    }

    // Access tuple operasyon adresini API sema anlamli konuma cevirir.
    private static Location CreateApiLocation(AccessTuple tuple)
    {
        return new Location
        {
            OperationId = tuple.OperationId,
            Method = tuple.Method,
            Path = tuple.Path
        };
    }

    // Profil database baglamasini DbSchemaName ve DbTableName anlamli konuma cevirir.
    private static Location CreateDatabaseLocation(ConceptBinding binding)
    {
        return new Location
        {
            DbSchemaName = binding.DbSchemaName,
            DbTableName = binding.TableName
        };
    }

    // Yol uzunlugunu hem hop hem toplam dugum butcesine karsi sinar.
    private static bool ExceedsBudget(EvidencePathDefinition path)
    {
        return path.Steps.Count > PtnBridgeConsts.MaxHopCount ||
               path.Steps.Count > PtnBridgeConsts.MaxNodeCount;
    }

    // Kanit sayisini uc degerli dugum durumuna cevirir.
    private static string StateFor(long observedCount)
    {
        return observedCount > 0 ? PtnEvidenceStateCodes.Observed : PtnEvidenceStateCodes.NotObserved;
    }

    // Sonuc hukumunu dogrudan etkileyen dugum turlerini yuksek alaka olarak isaretler.
    private static bool IsHighRelevance(string nodeKindCode)
    {
        return nodeKindCode is PtnNodeKindCodes.ScopeRequired or
            PtnNodeKindCodes.GrantMatched or
            PtnNodeKindCodes.AssertionDerivable;
    }

}
