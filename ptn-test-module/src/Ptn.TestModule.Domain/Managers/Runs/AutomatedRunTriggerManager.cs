using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Settings;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Zamanlanmis, webhook ve sozlesme degisikligi tetikleyicilerinin Pending kosumunu tek kuralla kurar.
// sistemdeki gorevi: Ayni tetikleyici referansinin ikinci kez kosum uretmesini engelleyen idempotency sahibidir.
/// <summary>
/// Oturum acmis kullanici olmadan baslayan kosumlarin olusturma ve tekrarlama kurallarini uygular.
/// </summary>
public class AutomatedRunTriggerManager : TestModuleDomainService
{
    private readonly TestRunManager _testRunManager;
    private readonly RunEnvironmentBindingManager _environmentBindingManager;
    private readonly ITestRunRepository _testRunRepository;
    private readonly ISettingProvider _settingProvider;

    public AutomatedRunTriggerManager(
        TestRunManager testRunManager,
        RunEnvironmentBindingManager environmentBindingManager,
        ITestRunRepository testRunRepository,
        ISettingProvider settingProvider)
    {
        _testRunManager = testRunManager;
        _environmentBindingManager = environmentBindingManager;
        _testRunRepository = testRunRepository;
        _settingProvider = settingProvider;
    }

    // Ayni tetikleyici daha once kosum urettiyse yeni kayit acmaz ve mevcut kosumu geri verir.
    /// <summary>Idempotency kapisini uygulayip yeni veya mevcut kosumu tek sonucta bildirir.</summary>
    public async Task<AutomatedRunOutcome> ResolveAsync(
        AutomatedRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var existing = await _testRunRepository.FindByTriggerAsync(
            request.TriggerKindCode,
            request.TriggerRef,
            request.IsScenarioScopedTrigger ? request.ScenarioId : null,
            cancellationToken);
        if (existing is not null)
        {
            return new AutomatedRunOutcome { Run = existing, IsNew = false };
        }

        var binding = await _environmentBindingManager.ResolveAsync(request.EnvironmentKey, cancellationToken);
        var created = await _testRunManager.CreateAsync(
            new TestRunCreateModel
            {
                ScenarioId = request.ScenarioId,
                TestKey = request.ScenarioKey,
                TriggerKindCode = request.TriggerKindCode,
                TriggerRef = request.TriggerRef,
                IsDryRun = false
            },
            binding,
            request.CanonicalInputs,
            specFingerprint: null,
            dbSchemaFingerprint: null,
            runnerRef: null,
            cancellationToken);
        return new AutomatedRunOutcome { Run = created, IsNew = true };
    }

    // Otomatik yollar icin ortam anahtari kullanicidan degil tenant ayarindan gelir.
    /// <summary>Otomatik tetikleyicilerin kullanacagi mantiksal ortam anahtarini cozer.</summary>
    public async Task<string> ResolveAutomationEnvironmentKeyAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(TestModuleSettings.AutomationEnvironmentKey);
        return string.IsNullOrWhiteSpace(configured)
            ? TestModuleRunSettingNames.DefaultAutomationEnvironmentKey
            : configured.Trim();
    }
}
