using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Settings;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Gelen webhook cagrisinin paylasilan sir dogrulamasini ve teslim kimligi normalizasyonunu yonetir.
// sistemdeki gorevi: Sir tanimlanmadan uc acilmaz; deger karsilastirilir ama hicbir yanitta, hatada veya logda tasinmaz.
/// <summary>
/// Webhook tetikleyicisinin kimlik dogrulama ve teslim kimligi kurallarini uygular.
/// </summary>
public class RunWebhookManager : TestModuleDomainService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ITestScenarioRepository _scenarioRepository;
    private readonly ITestScenarioStateRepository _stateRepository;
    private readonly AutomatedRunTriggerManager _automatedRunTriggerManager;

    public RunWebhookManager(
        ISettingProvider settingProvider,
        ITestScenarioRepository scenarioRepository,
        ITestScenarioStateRepository stateRepository,
        AutomatedRunTriggerManager automatedRunTriggerManager)
    {
        _settingProvider = settingProvider;
        _scenarioRepository = scenarioRepository;
        _stateRepository = stateRepository;
        _automatedRunTriggerManager = automatedRunTriggerManager;
    }

    // Teslim kimligini, yayinlanmis senaryoyu ve ortam anahtarini tek otomatik kosum istegine indirger.
    /// <summary>Webhook girdisini dogrulanmis otomatik kosum istegine cevirir.</summary>
    public async Task<AutomatedRunRequest> CreateRunRequestAsync(
        WebhookRunTriggerModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var scenario = await FindPublishedScenarioAsync(model.ScenarioKey, cancellationToken);
        return new AutomatedRunRequest
        {
            ScenarioId = scenario.Id,
            ScenarioKey = scenario.ScenarioKey,
            TriggerKindCode = TestTriggerKindCodes.Webhook,
            TriggerRef = NormalizeDeliveryId(model.DeliveryId),
            EnvironmentKey = await ResolveEnvironmentKeyAsync(model.EnvironmentKey),
            CanonicalInputs = scenario.CompiledHash,

            // Teslim kimligi gonderen sistemde zaten benzersizdir; tekrar senaryodan bagimsiz reddedilir.
            IsScenarioScopedTrigger = false
        };
    }

    // Yayinlanmamis veya bilinmeyen senaryo anahtarini kararli kodla reddeder.
    /// <summary>Senaryo anahtarinin yayinlanmis surumunu getirir.</summary>
    private async Task<TestScenario> FindPublishedScenarioAsync(
        string scenarioKey,
        CancellationToken cancellationToken)
    {
        var normalized = scenarioKey.Trim().ToLowerInvariant();
        var publishedState = await _stateRepository.FindAsync(
            new RepositoryQuery<TestScenarioState>().Where(item => item.Code == TestScenarioStateCodes.Published),
            cancellationToken);
        if (publishedState is null)
        {
            throw new EntityNotFoundException(typeof(TestScenarioState));
        }

        var scenario = await _scenarioRepository.FindPublishedAsync(normalized, publishedState.Id, cancellationToken);
        return scenario ?? throw new BusinessException(TestModuleRunErrorCodes.WebhookScenarioNotPublished)
            .WithData(nameof(scenarioKey), normalized);
    }

    // Ortam anahtari cagirandan gelebilir; gelmezse otomasyon ayarindaki ortam kullanilir.
    /// <summary>Webhook cagrisinin kullanacagi mantiksal ortam anahtarini cozer.</summary>
    private async Task<string> ResolveEnvironmentKeyAsync(string? environmentKey)
    {
        return string.IsNullOrWhiteSpace(environmentKey)
            ? await _automatedRunTriggerManager.ResolveAutomationEnvironmentKeyAsync()
            : environmentKey.Trim();
    }

    // Ayar tanimli degilse uc kapalidir; tanimliysa gelen deger sabit zamanli karsilastirilir.
    /// <summary>Gelen paylasilan sirri tenant ayarindaki degerle dogrular.</summary>
    public async Task EnsureAuthorizedAsync(string? presentedSecret, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configured = await _settingProvider.GetOrNullAsync(TestModuleSettings.WebhookSecret);
        if (string.IsNullOrWhiteSpace(configured) || !Matches(configured, presentedSecret))
        {
            throw new AbpAuthorizationException(TestModuleRunErrorCodes.WebhookSecretRejected);
        }
    }

    // Teslim kimligi tetikleyici referansi olarak satira yazilacagi icin bicimi kapida sabitlenir.
    /// <summary>Teslim kimligini kanonik ve satira sigan bicime getirir.</summary>
    public static string NormalizeDeliveryId(string? deliveryId)
    {
        var normalized = deliveryId?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Length > TestModuleRunSettingNames.MaxWebhookDeliveryIdLength)
        {
            throw new BusinessException(TestModuleRunErrorCodes.WebhookDeliveryIdInvalid);
        }

        return normalized;
    }

    // Sir uzunlugunu sizdirmemek icin sabit zamanli bayt karsilastirmasi kullanir.
    /// <summary>Iki sir degerinin esit olup olmadigini zamanlama sizintisi vermeden bildirir.</summary>
    private static bool Matches(string configured, string? presented)
    {
        if (string.IsNullOrEmpty(presented))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configured),
            Encoding.UTF8.GetBytes(presented));
    }
}
