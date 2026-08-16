using System;
using Ptn.TestModule.Constants.Catalog.Lookups;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Bes senaryo yayin kapisinin kapali ve kodlu kararlarini dogrular.
// sistemdeki gorevi: Turetilebilirlik, assertion, malzeme ve kaynak tutarliliginin uyarida kaybolmasini engeller.
public class ScenarioPublicationGateTests
{
    private static readonly Guid SpecSnapshotId = Guid.NewGuid();

    // Turetilemeyen assertion kaniti Published kararini kapatmalidir.
    [Fact]
    public void Should_reject_non_derivable_assertions()
    {
        var decision = Evaluate(CreateScenario(), areAssertionsDerivable: false);

        decision.IsPublishable.ShouldBeFalse();
        decision.FailedGateCodes.ShouldContain(ScenarioGateCodes.Derivability);
    }

    // Redocly lint kirmizi dondugunde sema kapisi dusmelidir (ADR-0015 §C).
    [Fact]
    public void Should_reject_when_the_lint_gate_is_red()
    {
        var decision = Evaluate(CreateScenario(), isSchemaValid: false);

        decision.IsPublishable.ShouldBeFalse();
        decision.FailedGateCodes.ShouldContain(ScenarioGateCodes.SchemaValidity);
    }

    // Bes makine kaniti da olumluysa senaryo Published karari alabilmelidir.
    [Fact]
    public void Should_publish_when_every_machine_gate_passes()
    {
        var decision = Evaluate(CreateScenario());

        decision.IsPublishable.ShouldBeTrue();
        decision.FailedGateCodes.ShouldBeEmpty();
    }

    // Sema lint uyarisi karar cevabina girmeli fakat tek basina yayin kapisini dusurmemelidir.
    [Fact]
    public void Should_report_schema_lint_warning_without_rejecting_publication()
    {
        var decision = Evaluate(CreateScenario(), schemaLintWarningCode: PtnSchemaLintWarningCodes.MissingPrimaryKey);

        decision.IsPublishable.ShouldBeTrue();
        decision.FailedGateCodes.ShouldBeEmpty();
        decision.Warnings.ShouldBe([PtnSchemaLintWarningCodes.MissingPrimaryKey]);
    }

    // Derleyici hic assertion uretmediyse senaryo Published kararini alamamalidir.
    [Fact]
    public void Should_reject_zero_assertion_count()
    {
        var decision = Evaluate(CreateScenario(), assertionCount: 0);

        decision.IsPublishable.ShouldBeFalse();
        decision.FailedGateCodes.ShouldContain(ScenarioGateCodes.AssertionCount);
    }

    // Dort malzeme bagindan biri eksikse butunluk kapisi dusmelidir.
    [Fact]
    public void Should_reject_incomplete_material_seal()
    {
        var decision = Evaluate(CreateScenario(includeDbSchemaFingerprint: false));

        decision.IsPublishable.ShouldBeFalse();
        decision.FailedGateCodes.ShouldContain(ScenarioGateCodes.MaterialIntegrity);
    }

    // SourceDescriptions baska spec snapshot'a bagliysa tutarlilik kapisi dusmelidir.
    [Fact]
    public void Should_reject_source_descriptions_from_another_spec()
    {
        var decision = Evaluate(CreateScenario(), sourceSnapshotId: Guid.NewGuid());

        decision.IsPublishable.ShouldBeFalse();
        decision.FailedGateCodes.ShouldContain(ScenarioGateCodes.SourceDescriptionConsistency);
    }

    // Birden cok kapi dustugunde karar kodlari zorunlu degerlendirme sirasini korumalidir.
    [Fact]
    public void Should_report_failed_gate_codes_in_evaluation_order()
    {
        var decision = Evaluate(
            CreateScenario(includeDbSchemaFingerprint: false),
            isSchemaValid: false,
            areAssertionsDerivable: false,
            assertionCount: 0,
            sourceSnapshotId: Guid.NewGuid());

        decision.FailedGateCodes.ShouldBe([
            ScenarioGateCodes.SchemaValidity,
            ScenarioGateCodes.Derivability,
            ScenarioGateCodes.AssertionCount,
            ScenarioGateCodes.MaterialIntegrity,
            ScenarioGateCodes.SourceDescriptionConsistency
        ]);
    }

    // Verilen aggregate ve derleyici kanitini gercek gate manager ile degerlendirir.
    private static TestScenarioPublishDecision Evaluate(
        ScenarioPublicationCandidate candidate,
        bool isSchemaValid = true,
        bool areAssertionsDerivable = true,
        int assertionCount = 1,
        Guid? sourceSnapshotId = null,
        string? schemaLintWarningCode = null)
    {
        return new ScenarioPublicationGateManager().Evaluate(
            candidate,
            new ScenarioCompilationEvidence
            {
                CompiledDocument = "compiled",
                CompiledHash = new string('b', 64),
                AssertionCount = assertionCount,
                IsSchemaValid = isSchemaValid,
                AreAssertionsDerivable = areAssertionsDerivable,
                SourceDescriptionSpecSnapshotIds = [sourceSnapshotId ?? SpecSnapshotId],
                SchemaLintWarnings = schemaLintWarningCode is null
                    ? []
                    : [new SchemaLintWarning { WarningCode = schemaLintWarningCode }]
            });
    }

    // Gate testlerine gereken en kucuk senaryo veri kabugunu kurar.
    private static ScenarioPublicationCandidate CreateScenario(bool includeDbSchemaFingerprint = true)
    {
        return new ScenarioPublicationCandidate
        {
            SourceDocument = "source",
            RulesFingerprint = Hash('c'),
            SpecSnapshotId = SpecSnapshotId,
            SpecFingerprint = Hash('d'),
            DbConnectionId = Guid.NewGuid(),
            DbSchemaFingerprint = includeDbSchemaFingerprint ? Hash('e') : null,
            ProfileFingerprint = Hash('f')
        };
    }

    // Test fingerprint'lerini DBML'nin 64 karakterlik digest biciminde uretir.
    private static string Hash(char value) => new(value, 64);
}
