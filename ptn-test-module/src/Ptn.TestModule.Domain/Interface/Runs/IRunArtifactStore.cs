using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Runs;

// islevi: Ihracat artefaktinin kalici blob deposundaki yazma, okuma ve silme sozlesmesini tanimlar.
// sistemdeki gorevi: Agir ihracat ciktisini satirdan cikarip test_run_results uzerinde yalniz blob adini birakir (PLAN-0003 TM-13, ADR-0016 §H).
/// <summary>
/// Kosum ihracat artefaktlarinin ABP BLOB Storing sinirini tanimlar.
/// </summary>
public interface IRunArtifactStore
{
    // Manager'in urettigi blob adiyla ihracat artefaktini kalici depoya yazar.
    /// <summary>Ihracat icerigini verilen blob adiyla saklayip adi geri dondurur.</summary>
    Task<string> SaveAsync(
        string blobName,
        string content,
        CancellationToken cancellationToken = default);

    // Saklanmis artefakti rapor yuzeyine ve CI tuketicisine geri okur.
    /// <summary>Verilen blob adindaki ihracat icerigini getirir; yoksa null doner.</summary>
    Task<string?> ReadAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    // Saklama suresi dolan artefakti kalici depodan birakir.
    /// <summary>Verilen blob adindaki ihracat artefaktini siler.</summary>
    Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default);
}
