using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Repositories;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: TestRun aggregate'inin trace, stale ve aktif ortam sorgu sozlesmesini tanimlar.
// sistemdeki gorevi: Kosum yasam dongusunun tum veritabani sorgularini provider katmaninda tutar.
/// <summary>
/// Test kosum aggregate'i icin gereken ozel repository sorgularini tanimlar.
/// </summary>
public interface ITestRunRepository : IBaseRepository<TestRun, Guid>
{
    Task<TestRunHeaderPage> GetHeaderPageAsync(
        TestRunQuery query,
        CancellationToken cancellationToken = default);

    Task<TestFindingHeaderPage> GetFindingPageAsync(
        TestFindingQuery query,
        CancellationToken cancellationToken = default);
    // W3C trace kimligiyle tek kosum kaydini bulur.
    /// <summary>Verilen trace kimligine sahip kosum kaydini getirir.</summary>
    Task<TestRun?> FindByTraceIdAsync(
        string traceId,
        CancellationToken cancellationToken = default);

    // Running durumunda esik zamandan once baslamis tum kosumlari tek sorguda getirir.
    /// <summary>Asili kabul edilen Running kosumlarini toplu olarak getirir.</summary>
    Task<IReadOnlyList<TestRun>> GetStaleRunningAsync(
        Guid runningStatusId,
        DateTime startedBefore,
        CancellationToken cancellationToken = default);

    /// <summary>HAR suresi dolmus tamamlanmis kosumlari sinirli bir dilim halinde getirir.</summary>
    Task<IReadOnlyList<TestRun>> GetExpiredHarArtifactsAsync(
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>Kosum saklama suresi dolmus tamamlanmis satirlari sinirli bir dilim halinde getirir.</summary>
    Task<IReadOnlyList<TestRun>> GetExpiredRunsAsync(
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>Silinen HAR blob'larinin kosum satirlarindaki referanslarini topluca temizler.</summary>
    Task ClearHarArtifactNamesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Saklama suresi dolmus kosumlari sonuclari ve bulgulariyla cascade siler.</summary>
    Task DeleteExpiredRunsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    // Kosumu ve en son terminal denemesini bulgulariyla birlikte tek Include sorgusunda getirir.
    /// <summary>Kosumun bulgulu ve teshisli terminal raporunu getirir.</summary>
    Task<TestRunReport?> GetReportAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // Kosumu, tum denemelerini ve bulgularini hukum kodu cozulmus halde tek sorguda getirir.
    /// <summary>Kosumun deterministik ihracat girdisini getirir.</summary>
    Task<RunExportSource?> GetExportSourceAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>Yayinlanmis senaryolarin bulgularindaki kural referanslarini senaryo sayilariyla gruplar.</summary>
    Task<IReadOnlyList<ScenarioCoverageRuleGroup>> GetRuleCoverageAsync(
        Guid publishedStateId,
        CancellationToken cancellationToken = default);

    /// <summary>Ayni tetikleyici turu, referansi ve senaryosu icin daha once uretilmis kosumu getirir.</summary>
    Task<TestRun?> FindByTriggerAsync(
        string triggerKindCode,
        string triggerRef,
        Guid? scenarioId,
        CancellationToken cancellationToken = default);
}
