using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Repositories;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Models.Catalog;

namespace Ptn.TestModule.Interface.Catalog;

// islevi: Senaryo surumu aggregate'inin veri erisim sozlesmesini tanimlar.
// sistemdeki gorevi: Son surum, yayinlanmis surum ve siradaki version sorgularini provider katmaninda tutar.
public interface ITestScenarioRepository : IBaseRepository<TestScenario, Guid>
{
    // Senaryo anahtarinin en yuksek surum numarali kaydini getirir.
    Task<TestScenario?> FindLatestVersionAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default);

    // Senaryo anahtarinin Published durumundaki en yeni surumunu getirir.
    Task<TestScenario?> FindPublishedAsync(
        string scenarioKey,
        Guid publishedStateId,
        CancellationToken cancellationToken = default);

    // Senaryo anahtari icin kullanilacak siradaki monoton surum numarasini uretir.
    Task<int> GetNextVersionNoAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default);

    /// <summary>Karantina suresi dolmus senaryolari tenant sinirlari otesinde sinirli bir dilim halinde getirir.</summary>
    Task<IReadOnlyList<TestScenario>> GetExpiredQuarantinesAsync(
        DateTime expiredBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>Vadesi gelmis, karantinada olmayan zamanlanmis senaryolari tenant sinirlari otesinde getirir.</summary>
    Task<IReadOnlyList<DueScenarioModel>> GetDueScheduledAsync(
        DateTime dueAt,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>Vadesi ilerletilecek senaryolari tek sorguda ve tenant sinirlari otesinde izlenebilir getirir.</summary>
    Task<IReadOnlyList<TestScenario>> GetManyForScheduleAdvanceAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Verilen snapshot'a muhurlenmis yayinlanmis ve karantinasiz senaryolari getirir.</summary>
    Task<IReadOnlyList<DueScenarioModel>> GetPublishedBySpecSnapshotAsync(
        Guid specSnapshotId,
        Guid publishedStateId,
        DateTime now,
        int maxResultCount,
        CancellationToken cancellationToken = default);
}
