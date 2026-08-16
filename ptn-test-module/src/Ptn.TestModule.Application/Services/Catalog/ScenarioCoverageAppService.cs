using System.Linq;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Mappers.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Yayinlanmis senaryolarin kapsam raporunun okunmasini orkestre eder.
// sistemdeki gorevi: Belge ayristirma ve gruplama Manager'a aittir; bu servis yalniz izin, cagri ve esleme yapar.
/// <summary>Senaryo kapsam raporu use-case'inin Application uygulamasidir.</summary>
[RemoteService(IsEnabled = false)]
public class ScenarioCoverageAppService : TestModuleAppService, IScenarioCoverageAppService
{
    /// <summary>Kapsam dikeyinin saf katmanlar-arasi eslemelerini yapar.</summary>
    private static readonly ScenarioCoverageMapper Mapper = new();
    private static readonly ApiOracleMapper ApiMapper = new();

    private readonly ScenarioCoverageManager _coverageManager;
    private readonly IApiOracleAppService _apiOracleAppService;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public ScenarioCoverageAppService(
        ScenarioCoverageManager coverageManager,
        IApiOracleAppService apiOracleAppService,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _coverageManager = coverageManager;
        _apiOracleAppService = apiOracleAppService;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini getirir.</summary>
    public async Task<ScenarioCoverageReportDto> GetCoverageAsync()
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Default);
        var token = _cancellationTokenProvider.Token;
        var report = await _coverageManager.BuildAsync(token);
        var inventoryTasks = report.Snapshots
            .Where(snapshot => snapshot.SpecSnapshotId.HasValue)
            .Select(snapshot => _apiOracleAppService.ListSnapshotOperationsAsync(
                snapshot.SpecSnapshotId!.Value, token));
        var inventories = (await Task.WhenAll(inventoryTasks))
            .Select(ApiMapper.MapResult)
            .ToList();
        _coverageManager.ApplyOperationInventories(report, inventories);
        return Mapper.Map(report);
    }
}
