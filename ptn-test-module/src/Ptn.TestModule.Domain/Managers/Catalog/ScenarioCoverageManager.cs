using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Catalog;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Catalog;

// islevi: Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini snapshot bazinda toplar.
// sistemdeki gorevi: Kapsamin payini ve checker envanterinden kanitli paydasini hesaplar.
/// <summary>
/// Senaryo kapsam raporunun pay tarafini derleme ve bulgu kayitlarindan uretir.
/// </summary>
public class ScenarioCoverageManager : TestModuleDomainService
{
    private readonly ITestScenarioRepository _scenarioRepository;
    private readonly ITestScenarioStateRepository _stateRepository;
    private readonly ITestRunRepository _testRunRepository;

    public ScenarioCoverageManager(
        ITestScenarioRepository scenarioRepository,
        ITestScenarioStateRepository stateRepository,
        ITestRunRepository testRunRepository)
    {
        _scenarioRepository = scenarioRepository;
        _stateRepository = stateRepository;
        _testRunRepository = testRunRepository;
    }

    // Yayinlanmis senaryolari ve bulgu kural referanslarini iki sorguda okuyup tek rapora indirger.
    /// <summary>Yayinlanmis senaryolarin kapsam raporunu uretir.</summary>
    public async Task<ScenarioCoverageReport> BuildAsync(CancellationToken cancellationToken = default)
    {
        var publishedStateId = await GetPublishedStateIdAsync(cancellationToken);
        var sources = await _scenarioRepository.GetPublishedCoverageSourcesAsync(
            publishedStateId,
            ScenarioCoverageConsts.MaxScenariosPerReport,
            cancellationToken);
        var rules = await _testRunRepository.GetRuleCoverageAsync(publishedStateId, cancellationToken);

        return new ScenarioCoverageReport
        {
            PublishedScenarioCount = sources.Count,
            Snapshots = GroupBySnapshot(sources),
            Rules = rules,

            // Payda checker'da operasyon envanteri ucu acilana kadar bilinmiyor kalir; sayi uydurulmaz.
            DenominatorState = ScenarioCoverageConsts.DenominatorUnknownState,
            DenominatorUnknownReason = ScenarioCoverageConsts.DenominatorUnknownReason
        };
    }

    // Eksiksiz checker envanterlerini snapshot gruplarina uygular ve global payda durumunu fail-closed cozer.
    public ScenarioCoverageReport ApplyOperationInventories(
        ScenarioCoverageReport report,
        IReadOnlyCollection<SnapshotOperationInventory> inventories)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(inventories);
        var bySnapshot = inventories.ToDictionary(item => item.SnapshotId);
        var unknownReason = report.Snapshots.Count == 0
            ? ScenarioCoverageConsts.DenominatorUnknownReason
            : null;
        foreach (var snapshot in report.Snapshots)
        {
            var snapshotUnknownReason = ApplyOperationInventory(snapshot, bySnapshot);
            unknownReason ??= snapshotUnknownReason;
        }

        report.DenominatorState = unknownReason is null
            ? ScenarioCoverageConsts.DenominatorKnownState
            : ScenarioCoverageConsts.DenominatorUnknownState;
        report.DenominatorUnknownReason = unknownReason ?? string.Empty;
        return report;
    }

    // Yayin durumunun lookup kimligini kapsam sorgularina cozer.
    /// <summary>Published senaryo durumunun lookup kimligini getirir.</summary>
    private async Task<Guid> GetPublishedStateIdAsync(CancellationToken cancellationToken)
    {
        var publishedState = await _stateRepository.FindAsync(
            new RepositoryQuery<TestScenarioState>()
                .Where(item => item.Code == TestScenarioStateCodes.Published),
            cancellationToken);
        return publishedState?.Id ?? throw new EntityNotFoundException(typeof(TestScenarioState));
    }

    // Senaryolari muhurlu olduklari snapshot'a gore gruplar ve operasyon kumelerini birlestirir.
    /// <summary>Kapsam kaynaklarini snapshot bazinda gruplar.</summary>
    private static List<ScenarioCoverageSnapshotGroup> GroupBySnapshot(IReadOnlyList<ScenarioCoverageSource> sources)
    {
        return
        [
            .. sources
                .GroupBy(source => source.SpecSnapshotId)
                .Select(group => new ScenarioCoverageSnapshotGroup
                {
                    SpecSnapshotId = group.Key,
                    ScenarioCount = group.Count(),
                    TouchedOperations = CollectOperations(group),

                    // Checker cevabi uygulanana kadar sayi uydurulmaz (RULE-0001).
                    TotalOperationCount = null
                })
                .OrderBy(group => group.SpecSnapshotId)
        ];
    }

    // Belge okuma tek sahiplidir; kapsam yalniz cikan adres kumelerini birlestirir.
    /// <summary>Bir snapshot grubundaki tum operasyon adreslerini toplar.</summary>
    private static List<string> CollectOperations(IEnumerable<ScenarioCoverageSource> sources)
    {
        var operations = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            operations.UnionWith(ArazzoCompilerManager.ReadTouchedOperations(source.CompiledDocument));
        }

        return [.. operations];
    }

    // Tek snapshot icin yalniz Passed ve eksiksiz envanterin gercek toplam sayisini kabul eder.
    private static string? ApplyOperationInventory(
        ScenarioCoverageSnapshotGroup snapshot,
        IReadOnlyDictionary<Guid, SnapshotOperationInventory> inventories)
    {
        if (!snapshot.SpecSnapshotId.HasValue)
        {
            return ScenarioCoverageConsts.SnapshotIdentityMissingReason;
        }
        if (!inventories.TryGetValue(snapshot.SpecSnapshotId.Value, out var inventory))
        {
            return ScenarioCoverageConsts.DenominatorUnknownReason;
        }
        if (inventory.OutcomeCode == PtnOutcomeCodes.SnapshotNotFound)
        {
            return ScenarioCoverageConsts.SnapshotNotFoundReason;
        }
        if (inventory.OutcomeCode != PtnOutcomeCodes.Passed ||
            !inventory.IsComplete ||
            inventory.TotalCount is < 0 or > int.MaxValue)
        {
            return ScenarioCoverageConsts.DenominatorUnknownReason;
        }

        snapshot.TotalOperationCount = (int)inventory.TotalCount;
        return null;
    }
}
