using System;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Bridge.Footprint;
using Shouldly;
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
