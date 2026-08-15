using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Runs;

// islevi: Kosum ihracatini yukleme, uretim, blob yazimi ve bag kalicilastirma sirasinda orkestre eder.
// sistemdeki gorevi: Karari Manager'da birakip yalniz BLOB sinirini ve kalicilik cagrisini tasir (PLAN-0003 TM-14/TM-30).
/// <summary>Kosum ihracat use-case'inin Application uygulamasidir.</summary>
[RemoteService(IsEnabled = false)]
public class RunReportExportService : TestModuleAppService, IRunReportExportService
{
    /// <summary>Kosum dikeyinin saf katmanlar-arasi eslemelerini yapar.</summary>
    private static readonly TestRunMapper Mapper = new();

    /// <summary>Ihracat kabul kapisini ve artefakt uretimini sahiplenen Manager'dir.</summary>
    private readonly RunExportManager _runExportManager;

    /// <summary>Terminal sonuc invariantlarini ve bag mutasyonunu sahiplenen Manager'dir.</summary>
    private readonly TestRunResultManager _testRunResultManager;

    /// <summary>Ihracat girdisini tek sorguda getiren kalicilik siniridir.</summary>
    private readonly ITestRunRepository _testRunRepository;

    /// <summary>Terminal sonuc kalicilik siniridir.</summary>
    private readonly ITestRunResultRepository _testRunResultRepository;

    /// <summary>Agir ihracat ciktisinin BLOB Storing siniridir.</summary>
    private readonly IRunArtifactStore _runArtifactStore;

    /// <summary>Aktif ABP istek iptal token'ini saglayan provider'dir.</summary>
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    /// <summary>Ihracat orkestrasyonunu Manager, repository ve blob bagimliliklariyla kurar.</summary>
    public RunReportExportService(
        RunExportManager runExportManager,
        TestRunResultManager testRunResultManager,
        ITestRunRepository testRunRepository,
        ITestRunResultRepository testRunResultRepository,
        IRunArtifactStore runArtifactStore,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _runExportManager = runExportManager;
        _testRunResultManager = testRunResultManager;
        _testRunRepository = testRunRepository;
        _testRunResultRepository = testRunResultRepository;
        _runArtifactStore = runArtifactStore;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>Kosumu tum standart formatlara ihrac edip baglari satira yazar.</summary>
    public async Task<RunArtifactLinksDto> ExportAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.Export);
        var cancellationToken = _cancellationTokenProvider.Token;
        var source = _runExportManager.EnsureExportable(
            await _testRunRepository.GetExportSourceAsync(id, cancellationToken),
            id);
        var result = _testRunResultManager.EnsureFound(
            await _testRunResultRepository.FindByAttemptAsync(id, cancellationToken: cancellationToken),
            id);
        var artifacts = _runExportManager.CreateArtifacts(source, result.Attempt);
        foreach (var artifact in artifacts)
        {
            await _runArtifactStore.SaveAsync(artifact.BlobName, artifact.Content, cancellationToken);
        }

        var links = RunExportManager.ToLinks(artifacts);
        TestRunResultManager.AttachArtifactLinks(result, links);
        await _testRunResultRepository.UpdateAsync(result, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(links);
    }
}
