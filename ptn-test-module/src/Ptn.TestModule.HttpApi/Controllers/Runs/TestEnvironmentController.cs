using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Runs;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Runs;

// islevi: Ortam listesi, ortam baglama yazimi ve sandbox reset use-case'lerini HTTP'ye acar.
// sistemdeki gorevi: Binding ve ayri permission metadata'sini AppService'e delege eder.
/// <summary>Test ortami baglama HTTP islemlerini sunar.</summary>
[Route(TestEnvironmentRoutes.Root)]
[ApiExplorerSettings(GroupName = TestEnvironmentRoutes.SwaggerGroupName)]
public class TestEnvironmentController : TestModuleController
{
    /// <summary>Ortam baglama AppService'ini lazy cozer.</summary>
    private ITestEnvironmentAppService AppService => LazyGetRequiredService<ITestEnvironmentAppService>();

    /// <summary>Tenant'in bagli test ortamlarini sir degeri tasimadan getirir.</summary>
    /// <returns>Ev standardi icinde ortam baglama listesi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Runs.View)]
    public async Task<Result<List<TestEnvironmentBindingDto>>> GetList()
    {
        var result = await AppService.GetListAsync();
        return result;
    }

    /// <summary>Tenant ortam haritasina yeni bir mantiksal ortam baglar.</summary>
    /// <param name="input">API ve veritabani hedeflerini tasiyan baglama girdisi.</param>
    /// <returns>Ev standardi icinde olusturulan ortam baglamasi.</returns>
    [HttpPost]
    [Authorize(TestModulePermissions.Runs.ManageEnvironments)]
    public async Task<Result<TestEnvironmentBindingDto>> Create([FromBody] CreateTestEnvironmentBindingDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    /// <summary>Bagli bir ortamin hedeflerini gunceller; mantiksal anahtar degismez.</summary>
    /// <param name="key">Guncellenecek mantiksal ortam anahtari.</param>
    /// <param name="input">Degistirilebilir ortam hedefleri.</param>
    /// <returns>Ev standardi icinde guncellenen ortam baglamasi.</returns>
    [HttpPut(TestEnvironmentRoutes.ByKey)]
    [Authorize(TestModulePermissions.Runs.ManageEnvironments)]
    public async Task<Result<TestEnvironmentBindingDto>> Update(
        string key,
        [FromBody] UpdateTestEnvironmentBindingDto input)
    {
        var result = await AppService.UpdateAsync(key, input);
        return result;
    }

    /// <summary>Bagli bir ortami tenant ortam haritasindan cikarir.</summary>
    /// <param name="key">Cikarilacak mantiksal ortam anahtari.</param>
    /// <returns>Basarili olursa iceriksiz ev standardi sonucu.</returns>
    [HttpDelete(TestEnvironmentRoutes.ByKey)]
    [Authorize(TestModulePermissions.Runs.ManageEnvironments)]
    public async Task<Result> Delete(string key)
    {
        await AppService.DeleteAsync(key);
        return Result.NoContent();
    }

    /// <summary>Yazma yetkili sandbox verisini kosumdan once sifirlar.</summary>
    /// <param name="key">Sandbox'i sifirlanacak mantiksal ortam anahtari.</param>
    /// <returns>Basarili olursa iceriksiz ev standardi sonucu.</returns>
    [HttpPost(TestEnvironmentRoutes.SandboxReset)]
    [Authorize(TestModulePermissions.Runs.SandboxReset)]
    public async Task<Result> ResetSandbox(string key)
    {
        await AppService.ResetSandboxAsync(key);
        return Result.NoContent();
    }
}
