using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: Senaryo saglik materialized view'inin salt-okunur sorgu ve yenileme sozlesmesini tanimlar.
// sistemdeki gorevi: View'a yazan hicbir yol acmaz; yenileme tek komuttur ve p95 hesabi veritabaninda kalir.
/// <summary>
/// Senaryo saglik gorunumunun okuma ve yenileme sinirini tanimlar.
/// </summary>
public interface IScenarioHealthRepository
{
    /// <summary>Aktif tenant'in saglik satirlarini filtreli ve kararli siralamayla sayfalar.</summary>
    Task<ScenarioHealthPage> GetPageAsync(
        ScenarioHealthQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Aktif tenant'in tek senaryo anahtarina ait saglik satirini getirir.</summary>
    Task<ScenarioHealth?> FindByScenarioKeyAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default);

    /// <summary>Materialized view'i okuyuculari bloklamadan yeniden hesaplar.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
