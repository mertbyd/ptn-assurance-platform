using System;
using NSubstitute;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Managers.Compilation;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Derlenmis belgeden dokunulan operasyon kumesinin cikarilmasini dogrular.
// sistemdeki gorevi: Kapsam raporunun pay tarafinin regresyon kapisidir; payda bu tarafta hesaplanmaz.
public class ScenarioCoverageManagerTests
{
    private const string CompiledDocument = """
        arazzo: 1.0.1
        sourceDescriptions:
          - name: databaseChecker
            url: http://db/openapi.json
        workflows:
          - workflowId: checkout
            steps:
              - stepId: create-order
                operationId: createOrder
              - stepId: read-order
                operationId: getOrder
              - stepId: assert-row
                operationPath: '{$sourceDescriptions.databaseChecker.url}#/paths/~1assertions~1row/post'
        """;

    // Sozlesme adimlarinin operationId degerleri tekil ve sirali dondurulmelidir.
    [Fact]
    public void Compiled_document_should_yield_its_touched_api_operations()
    {
        var operations = ArazzoCompilerManager.ReadTouchedOperations(CompiledDocument);

        operations.ShouldBe(["createOrder", "getOrder"]);
    }

    // Database Checker adimlari API operasyonu sayilmamalidir.
    [Fact]
    public void Database_checker_steps_should_not_count_as_api_operations()
    {
        var operations = ArazzoCompilerManager.ReadTouchedOperations(CompiledDocument);

        operations.ShouldNotContain(operation => operation.Contains("databaseChecker"));
    }

    // Bos veya bozuk belge kapsam raporunu kirmamalidir.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this: is: not: arazzo")]
    public void A_missing_or_broken_document_should_contribute_nothing(string document)
    {
        ArazzoCompilerManager.ReadTouchedOperations(document).ShouldBeEmpty();
    }

    // Eksiksiz Passed checker envanteri gercek toplam sayiyi ve Known durumunu rapora uygular.
    [Fact]
    public void Complete_snapshot_inventory_should_make_the_denominator_known()
    {
        var snapshotId = Guid.NewGuid();
        var report = Report(snapshotId);

        CreateManager().ApplyOperationInventories(report,
        [
            new SnapshotOperationInventory
            {
                SnapshotId = snapshotId,
                OutcomeCode = PtnOutcomeCodes.Passed,
                TotalCount = 7,
                IsComplete = true
            }
        ]);

        report.DenominatorState.ShouldBe(ScenarioCoverageConsts.DenominatorKnownState);
        report.DenominatorUnknownReason.ShouldBeEmpty();
        report.Snapshots[0].TotalOperationCount.ShouldBe(7);
    }

    // Checker'in bulamadigi snapshot icin yanlis sifir yerine aciklanabilir Unknown korur.
    [Fact]
    public void Missing_snapshot_should_keep_an_explainable_unknown_denominator()
    {
        var snapshotId = Guid.NewGuid();
        var report = Report(snapshotId);

        CreateManager().ApplyOperationInventories(report,
        [
            new SnapshotOperationInventory
            {
                SnapshotId = snapshotId,
                OutcomeCode = PtnOutcomeCodes.SnapshotNotFound,
                TotalCount = 0,
                IsComplete = true
            }
        ]);

        report.DenominatorState.ShouldBe(ScenarioCoverageConsts.DenominatorUnknownState);
        report.DenominatorUnknownReason.ShouldBe(ScenarioCoverageConsts.SnapshotNotFoundReason);
        report.Snapshots[0].TotalOperationCount.ShouldBeNull();
    }

    // Domain karar testleri icin persistence cagirmayan coverage Manager'ini kurar.
    private static ScenarioCoverageManager CreateManager() => new(
        Substitute.For<ITestScenarioRepository>(),
        Substitute.For<ITestScenarioStateRepository>(),
        Substitute.For<ITestRunRepository>());

    // Tek snapshot grubuyla denominator karari verilecek en kucuk raporu kurar.
    private static ScenarioCoverageReport Report(Guid snapshotId) => new()
    {
        Snapshots =
        [
            new ScenarioCoverageSnapshotGroup
            {
                SpecSnapshotId = snapshotId,
                TotalOperationCount = null
            }
        ],
        DenominatorState = ScenarioCoverageConsts.DenominatorUnknownState,
        DenominatorUnknownReason = ScenarioCoverageConsts.DenominatorUnknownReason
    };
}
