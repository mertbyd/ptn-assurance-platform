using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Veri gudumlu kanit zincirinin 403 yolu, Unavailable ve butce kararlarini dogrular.
// sistemdeki gorevi: Aciklama agaci ile hukmun ayni deterministik probe kaydindan uretilmesini saglar.
public class EvidenceChainManagerTests
{
    // 403 sinyalinde scope, subject, role ve grant adimlarini sirayla yurutup Confirmed verir.
    [Fact]
    public async Task Should_confirm_access_denied_path_from_four_evidence_nodes()
    {
        var fixture = CreateFixture(CreateProjectionResult);

        var result = await fixture.Engine.RunAsync(
            CreateTuple(),
            fixture.Pack.ProfileKey,
            CancellationToken.None);

        result.VerdictCode.ShouldBe(PtnVerdictCodes.Confirmed);
        var nodes = Flatten(result.Root).ToList();
        nodes.Select(node => node.NodeKindCode).ShouldBe(
            [PtnNodeKindCodes.ScopeRequired, PtnNodeKindCodes.SubjectResolved,
                PtnNodeKindCodes.RoleHeld, PtnNodeKindCodes.GrantMatched]);
        nodes.ShouldAllBe(node => node.Evidence.Count > 0);
    }

    // Projeksiyon okunamadiginda yokluk hukmu vermek yerine Unavailable ve Inconclusive dondurur.
    [Fact]
    public async Task Should_return_inconclusive_when_projection_is_unavailable()
    {
        var fixture = CreateFixture(_ => new PtnProjectionResult
        {
            StateCode = PtnEvidenceStateCodes.Unavailable
        });

        var result = await fixture.Engine.RunAsync(
            CreateTuple(),
            fixture.Pack.ProfileKey,
            CancellationToken.None);

        result.VerdictCode.ShouldBe(PtnVerdictCodes.Inconclusive);
        Flatten(result.Root).ShouldContain(node => node.StateCode == PtnEvidenceStateCodes.Unavailable);
        result.VerdictCode.ShouldNotBe(PtnVerdictCodes.RuledOut);
    }

    // Hop butcesini asan profil yolunu hicbir checker cagrisi yapmadan Inconclusive olarak kapatir.
    [Fact]
    public async Task Should_stop_when_hop_budget_is_exceeded()
    {
        var pack = CreatePack();
        pack.Paths[0].Steps = Enumerable.Range(0, 7).Select(_ =>
            new PtnEvidencePathStep
            {
                NodeKindCode = PtnNodeKindCodes.ScopeRequired,
                SourceCode = PtnEvidenceSourceCodes.ApiFailureIdentity
            }).ToList();
        var fixture = CreateFixture(CreateProjectionResult, pack);

        var result = await fixture.Engine.RunAsync(
            CreateTuple(),
            fixture.Pack.ProfileKey,
            CancellationToken.None);

        result.BudgetExceeded.ShouldBeTrue();
        result.VerdictCode.ShouldBe(PtnVerdictCodes.Inconclusive);
        await fixture.Diagnosis.DidNotReceiveWithAnyArgs()
            .DiagnoseApiAsync(default!, default);
    }

    // Test portlari ve profil manager'i ile gercek EvidenceChainManager sahipligini kurar.
    private static Fixture CreateFixture(
        Func<PtnProjectionRequest, PtnProjectionResult> projection,
        PtnProfilePack? suppliedPack = null)
    {
        var pack = suppliedPack ?? CreatePack();
        var provider = Substitute.For<IProfilePackProvider>();
        provider.LoadAsync(pack.ProfileKey, Arg.Any<CancellationToken>()).Returns(pack);
        var schema = Substitute.For<ISchemaKnowledgePort>();
        schema.GetSchemaFingerprintAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(pack.DbSchemaFingerprint);
        var profileManager = new ProfilePackManager(provider, schema);
        var database = Substitute.For<IDatabaseOraclePort>();
        database.ProjectAsync(Arg.Any<PtnProjectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => projection(call.Arg<PtnProjectionRequest>()));
        var diagnosis = CreateDiagnosisPort();
        var manager = new EvidenceChainManager(
            profileManager,
            Substitute.For<IApiOraclePort>(),
            database,
            diagnosis,
            schema);
        return new Fixture(manager, pack, diagnosis);
    }

    // API failure identity portunu gerekli scope olgusunu ve kaynakli kaniti dondurecek sekilde kurar.
    private static IFailureDiagnosisPort CreateDiagnosisPort()
    {
        var port = Substitute.For<IFailureDiagnosisPort>();
        port.DiagnoseApiAsync(Arg.Any<PtnDiagnosisRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PtnDiagnosisReport
            {
                Facts = new Dictionary<string, List<string>>
                {
                    [PtnDiagnosisFactCodes.ChallengeScopes] = ["ticket.read"]
                },
                Hypotheses =
                [
                    new PtnDiagnosisHypothesis
                    {
                        Ref = new PtnFindingRef
                        {
                            SourceCheckerCode = PtnSourceCheckerCodes.ApiContract,
                            Fingerprint = "sha256:api-evidence"
                        }
                    }
                ]
            });
        return port;
    }

    // Projeksiyon tablosuna gore subject, role veya grant olgusunu sirali satir olarak dondurur.
    private static PtnProjectionResult CreateProjectionResult(PtnProjectionRequest request)
    {
        var row = request.TableName switch
        {
            "users" => new Dictionary<string, string?> { ["id"] = "user-1" },
            "user_roles" => new Dictionary<string, string?> { ["role_id"] = "role-1" },
            _ => new Dictionary<string, string?> { ["permission_name"] = "ticket.write" }
        };
        return new PtnProjectionResult
        {
            StateCode = PtnEvidenceStateCodes.Observed,
            Rows = [row],
            ObservedRowCount = 1
        };
    }

    // access-denied-403 yolunu ve gerekli uc onayli kavram baglamasini olusturur.
    private static PtnProfilePack CreatePack()
    {
        return new PtnProfilePack
        {
            ProfileKey = "access-profile",
            Revision = "1",
            DbSchemaFingerprint = "sha256:schema",
            Bindings = [CreateSubjectBinding(), CreateRoleBinding(), CreateGrantBinding()],
            Paths = [CreatePath()]
        };
    }

    // Subject kavramini identity.users tablosuna onayli olarak baglar.
    private static PtnConceptBinding CreateSubjectBinding()
    {
        return new PtnConceptBinding
        {
            ConceptCode = PtnConceptCodes.Subject,
            DbSchemaName = "identity",
            TableName = "users",
            ColumnMap = new Dictionary<string, string> { [PtnBindingColumnCodes.Identity] = "id" },
            PatternCode = PtnBindingPatternCodes.SemanticEntity,
            StateCode = PtnBindingStateCodes.Approved
        };
    }

    // RoleAssignment kavramini subject ve role kolonlariyla onayli olarak baglar.
    private static PtnConceptBinding CreateRoleBinding()
    {
        return new PtnConceptBinding
        {
            ConceptCode = PtnConceptCodes.RoleAssignment,
            DbSchemaName = "identity",
            TableName = "user_roles",
            ColumnMap = new Dictionary<string, string>
            {
                [PtnBindingColumnCodes.Subject] = "user_id",
                [PtnBindingColumnCodes.Role] = "role_id"
            },
            PatternCode = PtnBindingPatternCodes.SemanticRoleAssignment,
            StateCode = PtnBindingStateCodes.Approved
        };
    }

    // PermissionGrant kavramini role ve permission kolonlariyla onayli olarak baglar.
    private static PtnConceptBinding CreateGrantBinding()
    {
        return new PtnConceptBinding
        {
            ConceptCode = PtnConceptCodes.PermissionGrant,
            DbSchemaName = "identity",
            TableName = "role_permission_grants",
            ColumnMap = new Dictionary<string, string>
            {
                [PtnBindingColumnCodes.Role] = "role_id",
                [PtnBindingColumnCodes.Permission] = "permission_name"
            },
            PatternCode = PtnBindingPatternCodes.SemanticRoleRelation,
            StateCode = PtnBindingStateCodes.Approved
        };
    }

    // Dordu de veriyle tanimli access-denied-403 kanit adimlarini olusturur.
    private static PtnEvidencePathDefinition CreatePath()
    {
        return new PtnEvidencePathDefinition
        {
            PathKey = "access-denied-403",
            Trigger = new PtnEvidencePathTrigger { StatusCodes = [403] },
            Steps =
            [
                CreateStep(PtnNodeKindCodes.ScopeRequired, PtnEvidenceSourceCodes.ApiFailureIdentity),
                CreateStep(PtnNodeKindCodes.SubjectResolved, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.Subject),
                CreateStep(PtnNodeKindCodes.RoleHeld, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.RoleAssignment, PtnNodeKindCodes.SubjectResolved),
                CreateStep(PtnNodeKindCodes.GrantMatched, PtnEvidenceSourceCodes.DatabaseProjection, PtnConceptCodes.PermissionGrant, PtnNodeKindCodes.RoleHeld)
            ],
            ConfirmedWhen = "ScopeRequired.observed && !GrantMatched.containsAny(ScopeRequired.values)",
            InconclusiveWhen = "any(step.state == Unavailable)"
        };
    }

    // Tek kanit yolu adimini kaynak, kavram ve istege bagli join referansiyla olusturur.
    private static PtnEvidencePathStep CreateStep(
        string nodeKindCode,
        string sourceCode,
        string? conceptCode = null,
        string? joinFrom = null)
    {
        return new PtnEvidencePathStep
        {
            NodeKindCode = nodeKindCode,
            SourceCode = sourceCode,
            ConceptCode = conceptCode,
            JoinFromNodeKindCode = joinFrom
        };
    }

    // 403 teshis tetikleyicisi ve subject referansi bulunan access tuple olusturur.
    private static PtnAccessTuple CreateTuple()
    {
        return new PtnAccessTuple
        {
            ConnectionId = Guid.NewGuid(),
            SpecSnapshotId = Guid.NewGuid(),
            SubjectRef = "user-1",
            OperationId = "get-ticket",
            Method = "GET",
            Path = "/tickets/{id}",
            StatusCode = 403
        };
    }

    // Aciklama agacini kokten tek-cocuk zinciri boyunca enumerable dugum listesine cevirir.
    private static IEnumerable<PtnExplanationNode> Flatten(PtnExplanationNode? root)
    {
        while (root is not null)
        {
            yield return root;
            root = root.Children.SingleOrDefault();
        }
    }

    // islevi: Manager, profil ve teshis mock'unu adlandirilmis test kurulumunda tasir.
    // sistemdeki gorevi: Uc iliskili fixture degerini tuple yerine tek tipte toplar.
    private sealed record Fixture(
        EvidenceChainManager Engine,
        PtnProfilePack Pack,
        IFailureDiagnosisPort Diagnosis);
}
