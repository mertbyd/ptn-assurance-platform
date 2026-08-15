using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Runs;

// islevi: Ihracat artefaktini ABP BLOB Storing container'ina yazar, geri okur ve siler.
// sistemdeki gorevi: Agir ciktiyi satirdan cikaran kalicilik siniridir; test_run_results uzerinde yalniz blob adi kalir (PLAN-0003 TM-13, ADR-0016 §H).
public sealed class RunArtifactService : IRunArtifactStore, ITransientDependency
{
    /// <summary>Ihracat artefaktlarinin adiyla cozulmus BLOB Storing container'idir.</summary>
    private readonly IBlobContainer _blobContainer;

    /// <summary>Blob adi ve boyut kapilarini sahiplenen Manager'dir.</summary>
    private readonly RunArtifactNameManager _nameManager;

    // Artefakt sinirini Domain.Shared'daki container adina bir kez baglar.
    public RunArtifactService(IBlobContainerFactory blobContainerFactory, RunArtifactNameManager nameManager)
    {
        _blobContainer = blobContainerFactory.Create(RunArtifactConsts.ContainerName);
        _nameManager = nameManager;
    }

    // Manager'in urettigi blob adiyla ihracati yazar ve adi cagirana geri verir.
    public async Task<string> SaveAsync(
        string blobName,
        string content,
        CancellationToken cancellationToken = default)
    {
        _nameManager.EnsureArtifactIsValid(blobName, content);
        await _blobContainer.SaveAsync(
            blobName,
            Encoding.UTF8.GetBytes(content),
            overrideExisting: true,
            cancellationToken);
        return blobName;
    }

    // Saklanmis artefakti rapor yuzeyine ve CI tuketicisine geri okur.
    public async Task<string?> ReadAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _nameManager.EnsureArtifactIsValid(blobName);
        var bytes = await _blobContainer.GetAllBytesOrNullAsync(blobName, cancellationToken);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    // Saklama suresi dolan artefakti kalici depodan birakir.
    public Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _nameManager.EnsureArtifactIsValid(blobName);
        return _blobContainer.DeleteAsync(blobName, cancellationToken);
    }
}
