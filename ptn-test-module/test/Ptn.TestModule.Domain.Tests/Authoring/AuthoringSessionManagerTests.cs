using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Authoring;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Models.Authoring;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Compilation;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Authoring;

// islevi: Kapali soru, tenant ve tek-adim mekanik birlestirme kurallarini dogrular.
// sistemdeki gorevi: Modelin grounding disi operasyon veya tam belge uretmesini domain kapisinda engeller.
public class AuthoringSessionManagerTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OperationReferenceId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TableReferenceId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // Kapali soru cevabini sonraki okumada korur ve grounded tek adimi mekanik Arazzo'ya birlestirir.
    [Fact]
    public void Should_answer_then_merge_one_grounded_step()
    {
        var manager = CreateManager();
        var session = CreateSession(manager);

        manager.Answer(session, TenantId, new AuthoringAnswerModel
        {
            QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
            SelectedOption = OperationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)
        });
        var read = manager.EnsureAvailable(session, TenantId);
        manager.AddStep(read, TenantId, new AuthoringStepModel
        {
            StepId = "create-ticket",
            OperationReferenceId = OperationReferenceId,
            RequestBodyJson = "{\"title\":\"broken printer\"}",
            AssertionPaths = ["/id"]
        });

        read.Answers[PtnOpenQuestionCodes.OperationReferenceRequired]
            .ShouldBe(OperationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat));
        read.Steps.Count.ShouldBe(1);
        read.SourceDocument.ShouldContain("arazzo: 1.0.1");
        read.SourceDocument.ShouldContain("stepId: create-ticket");
        read.SourceDocument.ShouldContain("~1tickets/post");
        read.SourceDocument.ShouldContain("$response.body#/id != null");
        read.SourceDocument.ShouldContain("broken printer");
    }

    // Cevaplanmamis kapali soruyla adim kabul etmeyerek model tahminini fail-closed tutar.
    [Fact]
    public void Should_reject_a_step_before_all_closed_questions_are_answered()
    {
        var exception = Should.Throw<BusinessException>(() => CreateManager().AddStep(
            CreateSession(CreateManager()),
            TenantId,
            new AuthoringStepModel
            {
                StepId = "create-ticket",
                OperationReferenceId = OperationReferenceId,
                AssertionPaths = ["/id"]
            }));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.AuthoringQuestionsUnanswered);
    }

    // Cache miss ve baska tenant kaydini ayri kararli hata kodlariyla reddeder.
    [Fact]
    public void Should_reject_expired_and_cross_tenant_sessions()
    {
        var manager = CreateManager();

        var expired = Should.Throw<BusinessException>(() =>
            manager.EnsureAvailable(null, TenantId));
        var crossTenant = Should.Throw<BusinessException>(() =>
            manager.EnsureAvailable(CreateSession(manager), Guid.NewGuid()));

        expired.Code.ShouldBe(TestModuleScenarioErrorCodes.AuthoringSessionNotFound);
        crossTenant.Code.ShouldBe(TestModuleScenarioErrorCodes.AuthoringSessionTenantMismatch);
    }

    // Oturum verilmeyen zemin cagrisi bugunku cevap seklini birebir korumalidir.
    [Fact]
    public void Should_leave_the_grounding_result_untouched_without_a_session()
    {
        var grounding = new GroundingResult { DecisionCode = "Grounded" };

        var attached = CreateManager().Attach(grounding, session: null);

        attached.SessionId.ShouldBeNull();
        attached.StepCount.ShouldBe(0);
        attached.PendingQuestionCodes.ShouldBeEmpty();
        attached.DecisionCode.ShouldBe("Grounded");
    }

    // Oturum verildiginde zemin cevabi oturum kimligini ve bekleyen kapali sorulari tasimalidir.
    [Fact]
    public void Should_report_the_session_identity_and_the_pending_questions()
    {
        var manager = CreateManager();
        var session = CreateSession(manager);

        var attached = manager.Attach(new GroundingResult(), manager.Continue(session, TenantId, model: null));

        attached.SessionId.ShouldBe(session.Id);
        attached.StepCount.ShouldBe(0);
        attached.PendingQuestionCodes.ShouldBe([PtnOpenQuestionCodes.OperationReferenceRequired]);
    }

    // Zemin cagrisindaki tek-adim onerisi ayni AddStep yolundan gecip belgeye islenmelidir.
    [Fact]
    public void Should_merge_the_proposed_step_through_the_same_add_step_path()
    {
        var manager = CreateManager();
        var session = CreateSession(manager);
        manager.Answer(session, TenantId, new AuthoringAnswerModel
        {
            QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
            SelectedOption = OperationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)
        });

        var continued = manager.Continue(session, TenantId, new AuthoringStepModel
        {
            StepId = "create-ticket",
            OperationReferenceId = OperationReferenceId,
            AssertionPaths = ["/id"]
        });
        var attached = manager.Attach(new GroundingResult(), continued);

        continued.Steps.Count.ShouldBe(1);
        continued.SourceDocument.ShouldContain("stepId: create-ticket");
        attached.StepCount.ShouldBe(1);
        attached.PendingQuestionCodes.ShouldBeEmpty();
    }

    // Zemin yolundaki oneri de cevaplanmamis kapali soruda fail-closed reddedilmelidir.
    [Fact]
    public void Should_reject_a_proposed_step_before_the_closed_questions_are_answered()
    {
        var manager = CreateManager();

        var exception = Should.Throw<BusinessException>(() => manager.Continue(
            CreateSession(manager),
            TenantId,
            new AuthoringStepModel
            {
                StepId = "create-ticket",
                OperationReferenceId = OperationReferenceId,
                AssertionPaths = ["/id"]
            }));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.AuthoringQuestionsUnanswered);
    }

    // Tipli veritabani adimini mekanik x-checknexus-db uzantisina birlestirir.
    [Fact]
    public void Should_merge_one_typed_database_step()
    {
        var manager = CreateManager();
        var session = CreateAnsweredSession(manager);

        manager.AddDatabaseStep(session, TenantId, CreateDatabaseStep());

        session.DatabaseSteps.Count.ShouldBe(1);
        session.SourceDocument.ShouldContain("assertRow");
        session.SourceDocument.ShouldContain(TableReferenceId.ToString());
        session.SourceDocument.ShouldContain("status");
        session.SourceDocument.ShouldContain(PtnDatabaseMatcherCodes.Equals);
    }

    // Anahtar baglamalari kolon adiyla eslesir; buyuk-kucuk harf farki ayri anahtar uretmemelidir.
    [Fact]
    public void Should_normalize_database_key_bindings_case_insensitively()
    {
        var manager = CreateManager();
        var session = CreateAnsweredSession(manager);

        manager.AddDatabaseStep(session, TenantId, CreateDatabaseStep());

        session.DatabaseSteps[0].KeyBindings.ContainsKey("ID").ShouldBeTrue();
    }

    // Ayni adim kimligini ikinci kez kabul etmeyerek belgeyi cift adimdan korur.
    [Fact]
    public void Should_reject_a_duplicate_database_step_id()
    {
        var manager = CreateManager();
        var session = CreateAnsweredSession(manager);
        manager.AddDatabaseStep(session, TenantId, CreateDatabaseStep());

        var exception = Should.Throw<BusinessException>(() =>
            manager.AddDatabaseStep(session, TenantId, CreateDatabaseStep()));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.AuthoringStepAlreadyExists);
    }

    // Kapali sorusu cevaplanmis, adim eklemeye hazir oturumu kurar.
    private static AuthoringSession CreateAnsweredSession(AuthoringSessionManager manager)
    {
        var session = CreateSession(manager);
        manager.Answer(session, TenantId, new AuthoringAnswerModel
        {
            QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
            SelectedOption = OperationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)
        });
        return session;
    }

    // Kapali kumeden secilmis tek satir beklentisi tasiyan veritabani adimi kurar.
    private static AuthoringDatabaseStep CreateDatabaseStep() => new()
    {
        StepId = "verify-ticket-row",
        TableReferenceId = TableReferenceId,
        OperationCode = "assertRow",
        KeyBindings = { ["id"] = "$steps.create-ticket.outputs.id" },
        Expectations =
        [
            new AuthoringExpectation
            {
                ColumnName = "status",
                MatcherCode = PtnDatabaseMatcherCodes.Equals,
                Value = "Open"
            }
        ]
    };

    // Gercek grounding sorusu ve operasyon adayindan en kucuk cache session'ini kurar.
    private static AuthoringSession CreateSession(AuthoringSessionManager manager)
    {
        var snapshotId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        return manager.Create(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            TenantId,
            new AuthoringSessionCreateModel
            {
                WorkflowId = "ticket-lifecycle",
                WorkflowSummary = "Create one support ticket",
                ApiSourceUrl = $"./snapshots/{snapshotId:D}.openapi.yaml",
                DatabaseSourceUrl = "./database-checker.openapi.yaml"
            },
            new GroundRequest { SpecSnapshotId = snapshotId },
            new GroundingResult
            {
                Questions =
                [
                    new ClosedQuestion
                    {
                        QuestionCode = PtnOpenQuestionCodes.OperationReferenceRequired,
                        Options = [OperationReferenceId.ToString(PtnBridgeConsts.ReferenceIdFormat)]
                    }
                ],
                OperationBinding = new OperationBinding
                {
                    Suggestions =
                    [
                        new OperationSuggestion
                        {
                            ReferenceId = OperationReferenceId,
                            SourceOperationId = "create-ticket",
                            SourceMethod = "POST",
                            SourcePath = "/tickets"
                        }
                    ]
                }
            });
    }

    // Authoring belge uretimini process calistirmayan gercek compiler nesnesiyle kurar.
    private static AuthoringSessionManager CreateManager() => new(
        new ArazzoCompilerManager(new ProfilePackManager(), new LinterStub()));

    // Authoring yolu lint cagirmasa da compiler constructor sozlesmesini yalitilmis tutar.
    private sealed class LinterStub : IArazzoDocumentLinter
    {
        public Task<ArazzoLintResult> LintAsync(
            string document,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ArazzoLintResult { IsValid = true });
    }
}
