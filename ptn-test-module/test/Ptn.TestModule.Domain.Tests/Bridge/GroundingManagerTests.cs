using System;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Catalog.Lookups;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Bridge.Footprint;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Grounding kararinin gercek envanter, esik ve onayli tablo kanitlarini dogrular.
// sistemdeki gorevi: Operasyon veya tablo adinin tahminle uretilmesini domain test kapisinda engeller.
public class GroundingManagerTests
{
    // Esit skorlu gercek adaylarda tahmin etmez ve kapali secim sorusunu envanter referanslariyla kurar.
    [Fact]
    public void Should_return_a_closed_question_for_ambiguous_real_operations()
    {
        var manager = CreateManager();
        var request = CreateRequest("ticket");
        var inventory = CreateInventory(
            Operation("create-ticket", "POST", "/tickets", "11111111-1111-1111-1111-111111111111"),
            Operation("get-ticket", "GET", "/tickets/{id}", "22222222-2222-2222-2222-222222222222"));

        var selected = manager.ResolveOperation(request, inventory);
        var result = manager.Ground(
            request,
            CreatePack(),
            "schema-fingerprint",
            CreateCapability(),
            inventory,
            selected,
            null,
            null,
            null);

        selected.ShouldBeNull();
        result.DecisionCode.ShouldBe(PtnVerdictCodes.Inconclusive);
        result.OperationBinding!.Suggestions.Count.ShouldBe(2);
        result.Questions.ShouldContain(question =>
            question.QuestionCode == PtnOpenQuestionCodes.OperationReferenceRequired &&
            question.Options.Count == 2);
    }

    // Tek esik ustu adayi ve tek onayli tabloyu kanitlarla birlestirip Confirmed sonucu verir.
    [Fact]
    public void Should_confirm_a_uniquely_grounded_operation_with_request_and_table_evidence()
    {
        var manager = CreateManager();
        var request = CreateRequest("create ticket");
        var pack = CreatePack();
        var inventory = CreateInventory(
            Operation("create-ticket", "POST", "/tickets", "11111111-1111-1111-1111-111111111111"),
            Operation("get-order", "GET", "/orders/{id}", "22222222-2222-2222-2222-222222222222"));

        var operation = manager.ResolveOperation(request, inventory);
        var tableBinding = manager.ResolveTableBinding(request, pack, "schema-fingerprint");
        var result = manager.Ground(
            request,
            pack,
            "schema-fingerprint",
            CreateCapability(),
            inventory,
            operation,
            new RequestExample { OutcomeCode = PtnOutcomeCodes.Passed },
            tableBinding,
            new TableDescription { DbSchemaName = "support", TableName = "tickets" });

        operation!.OperationId.ShouldBe("create-ticket");
        result.DecisionCode.ShouldBe(PtnVerdictCodes.Confirmed);
        result.CriticalFactCode.ShouldBe(PtnOutcomeCodes.Passed);
        result.Questions.ShouldBeEmpty();
        result.RequestExample.ShouldNotBeNull();
        result.TableDescription.ShouldNotBeNull();
    }

    // Kaynak belge yoksa checker kaniti uydurmaz ve eski referanslari kapali soru olarak korur.
    [Fact]
    public void Should_keep_validate_inconclusive_when_publication_evidence_is_absent()
    {
        var manager = CreateManager();
        var request = CreateValidateRequest(includeSource: false);

        var candidate = manager.CreatePublicationCandidate(request, Hash('d'));
        var result = manager.Validate(
            request, CreatePack(), "schema-fingerprint", null, null);

        candidate.ShouldBeNull();
        result.IsPublishable.ShouldBeFalse();
        result.DecisionCode.ShouldBe(PtnVerdictCodes.Inconclusive);
        result.Questions.Count.ShouldBe(1);
    }

    // Mevcut bes gate gectiginde Bridge sonucu gercek Confirmed yayin karari vermelidir.
    [Fact]
    public void Should_confirm_validate_when_the_existing_publication_gate_passes()
    {
        var manager = CreateManager();
        var request = CreateValidateRequest();
        var candidate = manager.CreatePublicationCandidate(request, Hash('d'))!;
        var evidence = CreatePublicationEvidence();
        var decision = new ScenarioPublicationGateManager().Evaluate(candidate, evidence);

        var result = manager.Validate(
            request, CreatePack(), "schema-fingerprint", evidence, decision);

        result.IsPublishable.ShouldBeTrue();
        result.DecisionCode.ShouldBe(PtnVerdictCodes.Confirmed);
        result.AssertionCount.ShouldBe(1);
        result.FailedGateCodes.ShouldBeEmpty();
        result.Questions.ShouldBeEmpty();
    }

    // Turetilebilirlik kaniti dusen mevcut gate koduyla aciklanabilir RuledOut sonucu vermelidir.
    [Fact]
    public void Should_rule_out_validate_with_the_existing_failed_gate_codes()
    {
        var manager = CreateManager();
        var request = CreateValidateRequest();
        var candidate = manager.CreatePublicationCandidate(request, Hash('d'))!;
        var evidence = CreatePublicationEvidence(areAssertionsDerivable: false);
        var decision = new ScenarioPublicationGateManager().Evaluate(candidate, evidence);

        var result = manager.Validate(
            request, CreatePack(), "schema-fingerprint", evidence, decision);

        result.IsPublishable.ShouldBeFalse();
        result.DecisionCode.ShouldBe(PtnVerdictCodes.RuledOut);
        result.FailedGateCodes.ShouldBe([ScenarioGateCodes.Derivability]);
    }

    // Istemci muhur tasimasa da yayin adayi sunucudaki aktif profil icerigine baglanmalidir.
    [Fact]
    public void Should_seal_the_candidate_with_the_server_profile_fingerprint()
    {
        var manager = CreateManager();
        var request = CreateValidateRequest();
        request.MaterialSeal!.ProfileFingerprint = null;

        var candidate = manager.CreatePublicationCandidate(request, Hash('d'))!;

        candidate.ProfileFingerprint.ShouldBe(Hash('d'));
    }

    // Istemcinin tasidigi muhur sunucudakinden farkliysa yayin adayi kurulmadan reddedilmelidir.
    [Fact]
    public void Should_reject_a_client_profile_fingerprint_that_differs_from_the_server()
    {
        var manager = CreateManager();
        var request = CreateValidateRequest();

        var exception = Should.Throw<BusinessException>(
            () => manager.CreatePublicationCandidate(request, Hash('e')));

        exception.Code.ShouldBe(TestModuleBridgeErrorCodes.ProfileFingerprintMismatch);
    }

    // Gercek test bagimliliklariyla GroundingManager sahipligini kurar.
    private static GroundingManager CreateManager() => new(
        new ProfilePackManager(),
        new FootprintCapabilityManager());

    // Grounding icin gecerli ve drift etmeyen en kucuk profil paketini kurar.
    private static ProfilePack CreatePack() => new()
    {
        ProfileKey = "unit-profile",
        Revision = "1",
        DbSchemaFingerprint = "schema-fingerprint",
        Bindings =
        [
            new ConceptBinding
            {
                ConceptCode = PtnConceptCodes.Resource,
                DbSchemaName = "support",
                TableName = "tickets",
                PatternCode = PtnBindingPatternCodes.SemanticEntity,
                StateCode = PtnBindingStateCodes.Approved,
                ApprovedBy = "test"
            }
        ]
    };

    // Tek ground isteginin zorunlu profil, snapshot, baglanti ve niyet alanlarini kurar.
    private static GroundRequest CreateRequest(string stepIntent) => new()
    {
        ProfileKey = "unit-profile",
        SpecSnapshotId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        ConnectionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        StepIntent = stepIntent,
        ResponseFormat = PtnResponseFormatCodes.Detailed
    };

    // Validate testleri icin derlenebilir kaynak ve eksiksiz malzeme muhrunu kurar.
    private static ValidateRequest CreateValidateRequest(bool includeSource = true) => new()
    {
        ProfileKey = "unit-profile",
        SpecSnapshotId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        ConnectionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        SourceDocument = includeSource ? "arazzo: 1.0.1" : null,
        MaterialSeal = new TestScenarioMaterialSeal
        {
            RulesFingerprint = Hash('a'),
            SpecSnapshotId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            SpecFingerprint = Hash('b'),
            DbConnectionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            DbSchemaFingerprint = Hash('c'),
            ProfileFingerprint = Hash('d')
        },
        ResponseFormat = PtnResponseFormatCodes.Detailed
    };

    // Mevcut gate Manager'in bes olumlu kanitini en kucuk gercek modelle kurar.
    private static ScenarioCompilationEvidence CreatePublicationEvidence(
        bool areAssertionsDerivable = true) => new()
    {
        AssertionCount = 1,
        IsSchemaValid = true,
        AreAssertionsDerivable = areAssertionsDerivable,
        SourceDescriptionSpecSnapshotIds =
            [Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    };

    // Malzeme fingerprint'lerini katalog sozlesmesinin 64 karakterlik biciminde uretir.
    private static string Hash(char value) => new(value, 64);

    // Tam ve Passed operasyon envanterini verilen gercek satirlardan kurar.
    private static SnapshotOperationInventory CreateInventory(params SnapshotOperation[] items) => new()
    {
        OutcomeCode = PtnOutcomeCodes.Passed,
        TotalCount = items.Length,
        IsComplete = true,
        Items = [.. items]
    };

    // Test operasyon satirini kapali referans ve checker adresiyle kurar.
    private static SnapshotOperation Operation(string operationId, string method, string path, string referenceId) => new()
    {
        ReferenceId = Guid.Parse(referenceId),
        OperationId = operationId,
        Method = method,
        Path = path
    };

    // Grounding footprint sonucunun oracle olmadigini koruyan unavailable capability kurar.
    private static CapabilityLevel CreateCapability() => new()
    {
        FootprintStrengthCode = PtnFootprintStrengthCodes.Unavailable
    };
}
