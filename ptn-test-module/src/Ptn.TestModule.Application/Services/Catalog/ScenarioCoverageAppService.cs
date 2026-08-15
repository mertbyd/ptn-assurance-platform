using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Mappers.Catalog;
using Ptn.TestModule.Permissions;
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

    private readonly ScenarioCoverageManager _coverageManager;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public ScenarioCoverageAppService(
        ScenarioCoverageManager coverageManager,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _coverageManager = coverageManager;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini getirir.</summary>
    public async Task<ScenarioCoverageReportDto> GetCoverageAsync()
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Default);
        var report = await _coverageManager.BuildAsync(_cancellationTokenProvider.Token);
        return Mapper.Map(report);
    }
}
