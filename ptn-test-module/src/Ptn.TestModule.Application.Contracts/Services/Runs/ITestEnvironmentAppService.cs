using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Ortam baglama listesi ve yetkili sandbox reset use-case'lerini tanimlar.
// sistemdeki gorevi: Setting ve yazma yetkili sandbox portlarini public sozlesmeye baglar.
public interface ITestEnvironmentAppService : IApplicationService
{
    Task<List<TestEnvironmentBindingDto>> GetListAsync();
    Task ResetSandboxAsync(string key);
}
