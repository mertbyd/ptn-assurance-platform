using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Runs;

// islevi: HAR artefaktini ABP BLOB Storing container'ina yazar, geri okur ve siler.
// sistemdeki gorevi: Artefakti satirdan cikaran kalicilik siniridir; test_runs uzerinde yalniz har_blob_name kalir (ADR-0016 §H).
[ExposeServices(typeof(IHarArtifactStore))]
public sealed class HarArtifactService : IHarArtifactStore, ITransientDependency
{
    // HAR artefaktlarinin adiyla cozulmus BLOB Storing container'idir.
    private readonly IBlobContainer _blobContainer;
    private readonly WorkflowRunPlanner _planner;
    private readonly HarInterpreter _harInterpreter;

    // Artefakt sinirini Domain.Shared'daki container adina bir kez baglar.
    public HarArtifactService(
        IBlobContainerFactory blobContainerFactory,
        WorkflowRunPlanner planner,
        HarInterpreter harInterpreter)
    {
        _blobContainer = blobContainerFactory.Create(HarArtifactConsts.ContainerName);
        _planner = planner;
        _harInterpreter = harInterpreter;
    }

    // Manager'in urettigi blob adiyla maskelenmis artefakti yazar ve adi cagirana geri verir.
    public async Task<string> SaveAsync(
        string blobName,
        string harContent,
        CancellationToken cancellationToken = default)
    {
        _planner.EnsureArtifactIsValid(blobName, harContent);
        var redacted = _harInterpreter.Redact(harContent);
        await _blobContainer.SaveAsync(
            blobName,
            Encoding.UTF8.GetBytes(redacted),
            overrideExisting: true,
            cancellationToken);
        return blobName;
    }

    // Saklanmis artefakti raporlama ve yeniden yargi icin geri okur.
    public async Task<string?> ReadAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _planner.EnsureArtifactIsValid(blobName);
        var bytes = await _blobContainer.GetAllBytesOrNullAsync(blobName, cancellationToken);
        if (bytes is null)
        {
            return null;
        }

        var content = Encoding.UTF8.GetString(bytes);
        _planner.EnsureArtifactIsValid(blobName, content);
        return content;
    }

    // Saklama suresi dolan artefakti kalici depodan birakir.
    public Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _planner.EnsureArtifactIsValid(blobName);
        return _blobContainer.DeleteAsync(blobName, cancellationToken);
    }

    // Toplu purge teslimindeki adlari Manager ile dogrulayip BLOB adapter'ine birlikte iletir.
    public Task DeleteManyAsync(
        IReadOnlyCollection<string> blobNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var blobName in blobNames)
        {
            _planner.EnsureArtifactIsValid(blobName);
        }

        return Task.WhenAll(blobNames.Select(blobName =>
            _blobContainer.DeleteAsync(blobName, cancellationToken)));
    }
}
