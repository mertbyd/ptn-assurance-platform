using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Repositories;
using Ptn.TestModule.Entities.Catalog;

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
}
