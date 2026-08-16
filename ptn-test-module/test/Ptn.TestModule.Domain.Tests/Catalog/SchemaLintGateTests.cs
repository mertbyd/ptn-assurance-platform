using System;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Sema lint uyarisinin yayin kararinda advisory kaldigini dogrular.
// sistemdeki gorevi: DBX-07 risk bilgisinin RULE-0006 ret hukmune donusmesini engeller.
public class SchemaLintGateTests
{
    // Anahtarsiz tablo uyarisi raporlanmali fakat tum makine kapilari gecerliyse yayini reddetmemelidir.
    [Fact]
    public void Should_keep_schema_lint_warning_non_blocking()
    {
        var snapshotId = Guid.NewGuid();
        var decision = new ScenarioPublicationGateManager().Evaluate(
            CreateScenario(snapshotId),
            new ScenarioCompilationEvidence
            {
                AssertionCount = 1,
                IsSchemaValid = true,
                AreAssertionsDerivable = true,
                SourceDescriptionSpecSnapshotIds = [snapshotId],
                SchemaLintWarnings =
                [
                    new SchemaLintWarning
                    {
                        WarningCode = PtnSchemaLintWarningCodes.MissingPrimaryKey
                    }
                ]
            });

        decision.IsPublishable.ShouldBeTrue();
        decision.FailedGateCodes.ShouldBeEmpty();
        decision.Warnings.ShouldBe([PtnSchemaLintWarningCodes.MissingPrimaryKey]);
    }

    // Gate testine eksiksiz malzeme muhru tasiyan en kucuk senaryo kabugunu kurar.
    private static ScenarioPublicationCandidate CreateScenario(Guid snapshotId)
    {
        return new ScenarioPublicationCandidate
        {
            SourceDocument = "source",
            RulesFingerprint = Hash('b'),
            SpecSnapshotId = snapshotId,
            SpecFingerprint = Hash('c'),
            DbConnectionId = Guid.NewGuid(),
            DbSchemaFingerprint = Hash('d'),
            ProfileFingerprint = Hash('e')
        };
    }

    // Test fingerprint'lerini kalici modelin digest butcesinde uretir.
    private static string Hash(char value) => new(value, 64);
}
