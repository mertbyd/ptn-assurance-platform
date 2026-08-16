using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Ortam baglama okuma, yazma ve yetkili sandbox reset use-case'lerini tanimlar.
// sistemdeki gorevi: Setting ve yazma yetkili sandbox portlarini public sozlesmeye baglar.
public interface ITestEnvironmentAppService : IApplicationService
{
    // Tenant'in bagli test ortamlarini sir degeri tasimadan getirir.
    Task<List<TestEnvironmentBindingDto>> GetListAsync();

    // Tenant ortam haritasina yeni bir mantiksal ortam baglar.
    Task<TestEnvironmentBindingDto> CreateAsync(CreateTestEnvironmentBindingDto input);

    // Bagli bir ortamin hedeflerini gunceller; mantiksal anahtar degismez.
    Task<TestEnvironmentBindingDto> UpdateAsync(string key, UpdateTestEnvironmentBindingDto input);

    // Bagli bir ortami tenant ortam haritasindan cikarir.
    Task DeleteAsync(string key);

    // Yazma yetkili sandbox verisini kosumdan once sifirlar.
    Task ResetSandboxAsync(string key);
}
