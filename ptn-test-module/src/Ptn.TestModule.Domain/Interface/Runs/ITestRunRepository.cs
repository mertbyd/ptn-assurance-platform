using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Repositories;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: TestRun aggregate'inin trace, stale ve aktif ortam sorgu sozlesmesini tanimlar.
// sistemdeki gorevi: Kosum yasam dongusunun tum veritabani sorgularini provider katmaninda tutar.
/// <summary>
/// Test kosum aggregate'i icin gereken ozel repository sorgularini tanimlar.
/// </summary>
public interface ITestRunRepository : IBaseRepository<TestRun, Guid>
{
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

    // Kosumu ve en son terminal denemesini bulgulariyla birlikte tek Include sorgusunda getirir.
    /// <summary>Kosumun bulgulu ve teshisli terminal raporunu getirir.</summary>
    Task<TestRunReport?> GetReportAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // Ortamda Pending veya Running bir kosum bulunup bulunmadigini veritabaninda hesaplar.
    /// <summary>Verilen ortam ve durum kumesinde aktif kosum olup olmadigini bildirir.</summary>
    Task<bool> ExistsActiveForEnvironmentAsync(
        string environmentKey,
        IReadOnlyCollection<Guid> activeStatusIds,
        CancellationToken cancellationToken = default);
}
