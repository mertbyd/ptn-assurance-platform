using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
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

    /// <summary>Kimligi verilen terminal sonucu tum bulgulariyla getirir.</summary>
    /// <param name="id">Okunacak TestRunResult aggregate kimligi.</param>
    /// <returns>Bulgulu terminal sonuc gorunumu.</returns>
    Task<TestRunResultDto> GetResultAsync(Guid id);

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
