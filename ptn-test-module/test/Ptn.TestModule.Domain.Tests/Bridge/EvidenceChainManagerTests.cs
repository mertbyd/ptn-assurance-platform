using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Veri gudumlu kanit zincirinin hukum, Unavailable ve butce kararlarini dogrular.
// sistemdeki gorevi: Application'da toplanan gozlemlerin yalniz somut Manager'da birlestirilmesini korur.
public class EvidenceChainManagerTests
{
    // Sirali ve kaynakli dort gozlemi profil ifadesine gore Confirmed yapar.
    [Fact]
    public void Should_confirm_access_denied_path_from_four_evidence_nodes()
    {
        var pack = CreatePack();
        var manager = new EvidenceChainManager(new ProfilePackManager());

        var result = manager.Run(pack, CreateTuple(), CreateObservedNodes());

        result.VerdictCode.ShouldBe(PtnVerdictCodes.Confirmed);
        Flatten(result.Root).Select(node => node.NodeKindCode).ShouldBe(
            [PtnNodeKindCodes.ScopeRequired, PtnNodeKindCodes.SubjectResolved,
                PtnNodeKindCodes.RoleHeld, PtnNodeKindCodes.GrantMatched]);
    }

    // Unavailable gozleminde yokluk hukmu vermek yerine Inconclusive dondurur.
    [Fact]
    public void Should_return_inconclusive_when_observation_is_unavailable()
    {
        var pack = CreatePack();
        var nodes = CreateObservedNodes();
        nodes[2].StateCode = PtnEvidenceStateCodes.Unavailable;
        var manager = new EvidenceChainManager(new ProfilePackManager());

        var result = manager.Run(pack, CreateTuple(), nodes);

        result.VerdictCode.ShouldBe(PtnVerdictCodes.Inconclusive);
        result.VerdictCode.ShouldNotBe(PtnVerdictCodes.RuledOut);
    }

    // Hop butcesini asan profil yolunu gozlemleri degerlendirmeden Inconclusive kapatir.
    [Fact]
    public void Should_stop_when_hop_budget_is_exceeded()
    {
        var pack = CreatePack();
        pack.Paths[0].Steps = Enumerable.Range(0, 7)
            .Select(_ => CreateStep(PtnNodeKindCodes.ScopeRequired, PtnEvidenceSourceCodes.ApiFailureIdentity))
            .ToList();
        var manager = new EvidenceChainManager(new ProfilePackManager());

        var result = manager.Run(pack, CreateTuple(), []);

        result.BudgetExceeded.ShouldBeTrue();
        result.VerdictCode.ShouldBe(PtnVerdictCodes.Inconclusive);
    }

    // Profil yolundaki her adim icin kaynakli bir observed dugumu olusturur.
    private static List<PtnExplanationNode> CreateObservedNodes() =>
    [
        CreateNode(PtnNodeKindCodes.ScopeRequired, "ticket.read"),
        CreateNode(PtnNodeKindCodes.SubjectResolved, "user-1"),
        CreateNode(PtnNodeKindCodes.RoleHeld, "role-1"),
        CreateNode(PtnNodeKindCodes.GrantMatched, "ticket.write")
    ];

    // Tek observed degeri kaynak referansiyla birlikte kanit dugumune yerlestirir.
    private static PtnExplanationNode CreateNode(string nodeKindCode, string value) => new()
    {
        NodeKindCode = nodeKindCode,
        StateCode = PtnEvidenceStateCodes.Observed,
        Evidence =
        [
            new PtnEvidence
            {
                ProbeKindCode = PtnProbeKindCodes.BridgeAvailability,
                FactCode = PtnFactCodes.Present,
                ObservedValue = value,
                Ref = new PtnFindingRef()
            }
        ]
    };

    // access-denied-403 yolunu ve gerekli uc onayli kavram baglamasini olusturur.
    private static PtnProfilePack CreatePack() => new()
    {
        ProfileKey = "access-profile",
        Revision = "1",
        DbSchemaFingerprint = "sha256:schema",
        Bindings = [CreateBinding(PtnConceptCodes.Subject), CreateBinding(PtnConceptCodes.RoleAssignment),
            CreateBinding(PtnConceptCodes.PermissionGrant)],
        Paths = [CreatePath()]
    };

    // Tek kavrami gecerli desen ve Approved durumuyla profile baglar.
    private static PtnConceptBinding CreateBinding(string conceptCode) => new()
    {
        ConceptCode = conceptCode,
        DbSchemaName = "identity",
        TableName = conceptCode,
        PatternCode = conceptCode == PtnConceptCodes.RoleAssignment
            ? PtnBindingPatternCodes.SemanticRoleAssignment
            : conceptCode == PtnConceptCodes.PermissionGrant
                ? PtnBindingPatternCodes.SemanticRoleRelation
                : PtnBindingPatternCodes.SemanticEntity,
        StateCode = PtnBindingStateCodes.Approved
    };

    // Dordu de veriyle tanimli access-denied-403 kanit adimlarini olusturur.
    private static PtnEvidencePathDefinition CreatePath() => new()
    {
        PathKey = "access-denied-403",
        Trigger = new PtnEvidencePathTrigger { StatusCodes = [403] },
        Steps =
        [
            CreateStep(PtnNodeKindCodes.ScopeRequired, PtnEvidenceSourceCodes.ApiFailureIdentity),
            CreateStep(PtnNodeKindCodes.SubjectResolved, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.Subject),
            CreateStep(PtnNodeKindCodes.RoleHeld, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.RoleAssignment),
            CreateStep(PtnNodeKindCodes.GrantMatched, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.PermissionGrant)
        ],
        ConfirmedWhen = "ScopeRequired.observed && !GrantMatched.containsAny(ScopeRequired.values)",
        InconclusiveWhen = "any(step.state == Unavailable)"
    };

    // Tek kanit yolu adimini kaynak ve istege bagli kavram koduyla olusturur.
    private static PtnEvidencePathStep CreateStep(string nodeKindCode, string sourceCode, string? conceptCode = null) => new()
    {
        NodeKindCode = nodeKindCode,
        SourceCode = sourceCode,
        ConceptCode = conceptCode
    };

    // 403 teshis tetikleyicisi bulunan access tuple olusturur.
    private static PtnAccessTuple CreateTuple() => new()
    {
        ConnectionId = Guid.NewGuid(),
        SpecSnapshotId = Guid.NewGuid(),
        OperationId = "get-ticket",
        Method = "GET",
        Path = "/tickets/{id}",
        StatusCode = 403
    };

    // Aciklama agacini kokten tek-cocuk zinciri boyunca enumerable dugum listesine cevirir.
    private static IEnumerable<PtnExplanationNode> Flatten(PtnExplanationNode? root)
    {
        while (root is not null)
        {
            yield return root;
            root = root.Children.SingleOrDefault();
        }
    }
}
