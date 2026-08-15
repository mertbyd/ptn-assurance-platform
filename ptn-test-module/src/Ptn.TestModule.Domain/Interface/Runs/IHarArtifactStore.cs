using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ptn.TestModule.Interface.Runs;

// islevi: HAR artefaktinin kalici blob deposundaki yazma, okuma ve silme sozlesmesini tanimlar.
// sistemdeki gorevi: Artefakti satirdan cikarip test_runs uzerinde yalniz har_blob_name birakir (ADR-0016 §H).
/// <summary>
/// HAR artefaktlarinin ABP BLOB Storing sinirini tanimlar.
/// </summary>
public interface IHarArtifactStore
{
    // Manager'in urettigi blob adiyla artefakti kalici depoya yazar.
    /// <summary>HAR icerigini verilen blob adiyla saklayip adi geri dondurur.</summary>
    Task<string> SaveAsync(
        string blobName,
        string harContent,
        CancellationToken cancellationToken = default);

    // Saklanmis artefakti raporlama ve yeniden yargi icin geri okur.
    /// <summary>Verilen blob adindaki HAR icerigini getirir; yoksa null doner.</summary>
    Task<string?> ReadAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    // Saklama suresi dolan artefakti kalici depodan birakir.
    /// <summary>Verilen blob adindaki HAR artefaktini siler.</summary>
    Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>Verilen HAR artefaktlarini tek bir toplu port cagrisi olarak siler.</summary>
    Task DeleteManyAsync(
        IReadOnlyCollection<string> blobNames,
        CancellationToken cancellationToken = default);
}
