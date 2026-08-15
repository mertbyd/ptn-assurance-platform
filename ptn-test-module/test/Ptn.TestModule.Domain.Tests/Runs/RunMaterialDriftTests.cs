using System;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Catalog;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Dort senaryo malzemesinden birinin kaymasinda uretilen terminal hukmu dogrular.
// sistemdeki gorevi: Bayat senaryonun Failed veya Skipped sayilmasini engeller.
/// <summary>
/// RunMaterialDriftManager malzeme muhur ve terminal karar testleridir.
/// </summary>
public class RunMaterialDriftTests
{
    /// <summary>Test senaryosunun API snapshot kimligidir.</summary>
    private static readonly Guid SpecSnapshotId = Guid.NewGuid();

    // Tek DB semasi kaymasi malzemeyi adiyla Inconclusive ve Technical sonucuna cevirmelidir.
    /// <summary>Bir malzeme kaymasinin asla Failed olmayan terminal sonucunu dogrular.</summary>
    [Fact]
    public void Should_create_inconclusive_technical_result_for_material_drift()
    {
        var manager = new RunMaterialDriftManager();
        var drift = manager.Evaluate(
            CreateScenario(),
            Hash('a'),
            Hash('b'),
            Hash('9'),
            Hash('d'));

        var terminal = manager.CreateInconclusiveTerminalModel(drift);

        drift.HasDrift.ShouldBeTrue();
        drift.DriftedMaterialCodes.ShouldBe([TestRunConsts.DatabaseSchemaMaterialCode]);
        terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Inconclusive);
        terminal.OutcomeCode.ShouldNotBe(TestOutcomeStatusCodes.Failed);
        terminal.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Technical);
        terminal.Detail.ShouldNotBeNull();
        terminal.Detail.ShouldContain(TestRunConsts.DatabaseSchemaMaterialCode);
    }

    // Dort malzeme muhrunu dolu tasiyan yayinlanabilir senaryo veri kabugunu kurar.
    /// <summary>Malzeme karsilastirma testi icin muhurlu TestScenario olusturur.</summary>
    private static TestScenario CreateScenario()
    {
        return new TestScenario(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            null,
            new TestScenarioCreateModel
            {
                ScenarioKey = "runs.drift",
                Title = "Drift scenario",
                SourceDocument = "source",
                SourceHash = Hash('e'),
                MaterialSeal = new TestScenarioMaterialSeal
                {
                    RulesFingerprint = Hash('a'),
                    SpecSnapshotId = SpecSnapshotId,
                    SpecFingerprint = Hash('b'),
                    DbConnectionId = Guid.NewGuid(),
                    DbSchemaFingerprint = Hash('c'),
                    ProfileFingerprint = Hash('d')
                }
            });
    }

    // Test fingerprint'lerini DBML'nin 64 karakterlik digest biciminde uretir.
    /// <summary>Verilen karakterden 64 karakterlik test fingerprint'i olusturur.</summary>
    private static string Hash(char value) => new(value, 64);
}
