using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Settings;
using Volo.Abp.SettingManagement;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Runs;

// islevi: Sirsiz ortam listesini, tenant ortam haritasinin yazimini ve ayri izinli sandbox resetini orkestre eder.
// sistemdeki gorevi: Setting manager I/O'su ile sandbox capability arasindaki public Application yuzeyidir.
public class TestEnvironmentAppService : TestModuleAppService, ITestEnvironmentAppService
{
    private static readonly TestRunMapper Mapper = new();
    private readonly RunEnvironmentBindingManager _bindingManager;
    private readonly ITestDataSandbox _sandbox;
    private readonly ISettingManager _settingManager;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    // Ortam karar sahibini, sandbox capability'sini ve tenant ayar yazma I/O'sunu tek akima baglar.
    public TestEnvironmentAppService(
        RunEnvironmentBindingManager bindingManager,
        ITestDataSandbox sandbox,
        ISettingManager settingManager,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _bindingManager = bindingManager;
        _sandbox = sandbox;
        _settingManager = settingManager;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Bagli ortamlari sir degeri acilmadan public listeye cevirir.
    public async Task<List<TestEnvironmentBindingDto>> GetListAsync()
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var bindings = await _bindingManager.ListAsync(_cancellationTokenProvider.Token);
        return Mapper.Map(new List<TestRunEnvironmentBinding>(bindings));
    }

    // Yeni ortami Manager'in urettigi kararli belgeyle tenant ayarina yazar.
    public async Task<TestEnvironmentBindingDto> CreateAsync(CreateTestEnvironmentBindingDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.ManageEnvironments);
        var binding = Mapper.Map(input);
        var configured = await _settingManager.GetOrNullForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings);
        var document = _bindingManager.Bind(configured, binding);
        await _settingManager.SetForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings, document);
        return Mapper.Map(binding);
    }

    // Bagli ortamin hedeflerini degistirir; mantiksal anahtar rotadan gelir.
    public async Task<TestEnvironmentBindingDto> UpdateAsync(string key, UpdateTestEnvironmentBindingDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.ManageEnvironments);
        var binding = Mapper.Map(input);
        var configured = await _settingManager.GetOrNullForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings);
        var document = _bindingManager.Rebind(configured, key, binding);
        await _settingManager.SetForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings, document);
        return Mapper.Map(binding);
    }

    // Ortami haritadan cikaran kararli belgeyi tenant ayarina yazar.
    public async Task DeleteAsync(string key)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.ManageEnvironments);
        var configured = await _settingManager.GetOrNullForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings);
        var document = _bindingManager.Unbind(configured, key);
        await _settingManager.SetForCurrentTenantAsync(TestModuleSettings.EnvironmentBindings, document);
    }

    // Ortami cozup yazma yetkili sandbox verisini kosumdan once sifirlar.
    public async Task ResetSandboxAsync(string key)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.SandboxReset);
        await _bindingManager.ResolveAsync(key, _cancellationTokenProvider.Token);
        await _sandbox.ResetAsync(key, _cancellationTokenProvider.Token);
    }
}
