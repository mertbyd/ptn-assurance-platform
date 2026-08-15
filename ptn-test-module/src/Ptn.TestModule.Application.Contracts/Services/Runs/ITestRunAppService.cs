using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Kosum olusturma, okuma, claim ve terminal yazma use-case'lerini tanimlar.
// sistemdeki gorevi: Persistence entity'lerini acmadan Application katmaninin public kosum sozlesmesini sunar.
/// <summary>Test kosumu use-case'lerinin Application sozlesmesidir.</summary>
public interface ITestRunAppService : IApplicationService
{
    /// <summary>Kimligi verilen test kosumunu getirir.</summary>
    /// <param name="id">Okunacak TestRun aggregate kimligi.</param>
    /// <returns>Guncel test kosumu gorunumu.</returns>
    Task<TestRunDto> GetAsync(Guid id);

    /// <summary>Test kosumlarini kararli siralama ve sayfalamayla getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Agir rapor kolonlari tasimayan kosum sayfasi.</returns>
    Task<PagedResultDto<TestRunHeaderDto>> GetListAsync(TestRunListInput input);

    /// <summary>Running kosuma kooperatif iptal talebi yazar.</summary>
    Task CancelAsync(Guid id);

    /// <summary>Terminal sonuc artefaktinin UTF-8 govdesini getirir.</summary>
    Task<RunArtifactContentDto> GetArtifactContentAsync(Guid id, string format);

    /// <summary>Kosumun HAR artefaktinin UTF-8 govdesini getirir.</summary>
    Task<RunArtifactContentDto> GetHarContentAsync(Guid id);

    /// <summary>Kimligi verilen terminal sonucu tum bulgulariyla getirir.</summary>
    /// <param name="id">Okunacak TestRunResult aggregate kimligi.</param>
    /// <returns>Bulgulu terminal sonuc gorunumu.</returns>
    Task<TestRunResultDto> GetResultAsync(Guid id);

    /// <summary>Kosumu terminal hukmu, bulgulari ve teshis raporuyla birlikte getirir.</summary>
    /// <param name="id">Raporu okunacak TestRun aggregate kimligi.</param>
    /// <returns>Bulgulu ve teshisli kosum raporu.</returns>
    Task<TestReportDetailDto> GetReportAsync(Guid id);

    /// <summary>Kuru kosumun sozlesmeyle celisip celismedigini yonlendirme vermeden bildirir.</summary>
    Task<DryRunContradictionReportDto> GetDryRunContradictionAsync(Guid id);

    /// <summary>Terminal sonucun ihracat artefaktlarina isaret eden baglarini getirir.</summary>
    /// <param name="id">Baglari okunacak TestRunResult aggregate kimligi.</param>
    /// <returns>Uc ihracat formatinin resource_link gorunumu.</returns>
    Task<RunArtifactLinksDto> GetArtifactLinksAsync(Guid id);

    /// <summary>Tenant ortam ayarini cozip yeni Pending kosum olusturur.</summary>
    /// <param name="input">Kosum, ortam ve fingerprint girdileri.</param>
    /// <returns>Kalicilastirilan Pending kosum.</returns>
    Task<TestRunDto> CreateAsync(CreateTestRunDto input);

    /// <summary>Yeni Pending kosumu olusturup dayanikli icra job'ini kuyruga verir.</summary>
    /// <param name="input">Kosum, ortam ve fingerprint girdileri.</param>
    /// <returns>Kuyruga verilen Pending kosum.</returns>
    Task<TestRunDto> TriggerAsync(CreateTestRunDto input);

    /// <summary>Pending kosumu idempotent bicimde Running durumuna claim eder.</summary>
    /// <param name="id">Claim edilecek TestRun aggregate kimligi.</param>
    /// <returns>Claim karari ve guncel kosum.</returns>
    Task<TestRunClaimDto> StartAsync(Guid id);

    /// <summary>Running kosuma terminal sonucu ve bulgulari atomik yazar.</summary>
    /// <param name="id">Terminale tasinacak TestRun aggregate kimligi.</param>
    /// <param name="input">Hukum, teshis, sure, artefakt ve bulgu girdileri.</param>
    /// <returns>Kalicilastirilan bulgulu terminal sonuc.</returns>
    Task<TestRunResultDto> WriteTerminalAsync(Guid id, WriteTestRunTerminalDto input);
}
