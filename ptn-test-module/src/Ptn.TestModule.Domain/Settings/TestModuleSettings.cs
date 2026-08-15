using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;

namespace Ptn.TestModule.Settings;

// islevi: Test Module ayar adlarini mevcut ABP setting provider yuzeyine aktarir.
// sistemdeki gorevi: Domain.Shared sahibi olan kopru ve tenant ortam ayarlarini mevcut modul ayar grubuyla uyumlu tutar.
/// <summary>
/// Test Module tarafindan kaydedilen ABP Setting adlarini tasir.
/// </summary>
public static class TestModuleSettings
{
    /// <summary>Test Module ayarlarinin ortak grup adidir.</summary>
    public const string GroupName = "TestModule";

    /// <summary>Kopru profil paketi kok yolu ayar adidir.</summary>
    public const string ProfilePackPath = PtnBridgeSettingNames.ProfilePackPath;

    /// <summary>Yayin aninda derlemeye malzeme olan aktif profil paketi anahtari ayar adidir.</summary>
    public const string ProfilePackKey = PtnBridgeSettingNames.ProfilePackKey;

    /// <summary>Test edilecek yazilimin is kurali belgesinin kok yolu ayar adidir.</summary>
    public const string BusinessRulesPath = PtnBridgeSettingNames.BusinessRulesPath;

    /// <summary>Tenant-scoped test ortami baglama haritasi ayar adidir.</summary>
    public const string EnvironmentBindings = TestModuleRunSettingNames.EnvironmentBindings;

    /// <summary>Kosumu icra eden pinli dis runner imaji ayar adidir.</summary>
    public const string RunnerImage = TestModuleRunSettingNames.RunnerImage;

    /// <summary>Tum senaryonun runner tarafindaki azami sure ayar adidir.</summary>
    public const string RunnerExecutionTimeoutSeconds = TestModuleRunSettingNames.RunnerExecutionTimeoutSeconds;

    /// <summary>Tek HTTP cagrisinin runner tarafindaki azami sure ayar adidir.</summary>
    public const string RunnerMaxFetchTimeoutSeconds = TestModuleRunSettingNames.RunnerMaxFetchTimeoutSeconds;

    /// <summary>HAR artefaktlarinin gun cinsinden saklama suresi ayar adidir.</summary>
    public const string HarRetentionDays = TestModuleRunSettingNames.HarRetentionDays;

    /// <summary>Tamamlanmis kosum satirlarinin gun cinsinden saklama suresi ayar adidir.</summary>
    public const string RunRetentionDays = TestModuleRunSettingNames.RunRetentionDays;

    /// <summary>Tek purge tesliminde islenecek azami kosum sayisi ayar adidir.</summary>
    public const string RunPurgeBatchSize = TestModuleRunSettingNames.RunPurgeBatchSize;

    /// <summary>Asili Running kosularinin kurtarilma esigi ayar adidir.</summary>
    public const string StaleRunThresholdMinutes = TestModuleRunSettingNames.StaleRunThresholdMinutes;

    /// <summary>Sandbox verisini kosumdan once temizleyen strateji ayar adidir.</summary>
    public const string SandboxResetStrategy = TestModuleRunSettingNames.SandboxResetStrategy;

    /// <summary>Ortam kilidini edinmek icin beklenecek azami sure ayar adidir.</summary>
    public const string RunConcurrencyWaitSeconds = TestModuleRunSettingNames.RunConcurrencyWaitSeconds;

    /// <summary>Suresi dolmus karantinalarin tarandigi periyot ayar adidir.</summary>
    public const string QuarantineSweepPeriodSeconds = TestModuleCatalogSettingNames.QuarantineSweepPeriodSeconds;

    /// <summary>Tek karantina tikinde temizlenecek azami senaryo sayisi ayar adidir.</summary>
    public const string QuarantineSweepBatchSize = TestModuleCatalogSettingNames.QuarantineSweepBatchSize;

    /// <summary>Saglik materialized view'inin yenilendigi periyot ayar adidir.</summary>
    public const string ScenarioHealthRefreshPeriodSeconds = TestModuleRunSettingNames.ScenarioHealthRefreshPeriodSeconds;

    /// <summary>Vadesi gelmis zamanlanmis senaryolarin tarandigi periyot ayar adidir.</summary>
    public const string ScheduleSweepPeriodSeconds = TestModuleCatalogSettingNames.ScheduleSweepPeriodSeconds;

    /// <summary>Tek zamanlama tikinde kuyruklanacak azami senaryo sayisi ayar adidir.</summary>
    public const string MaxScenariosPerTick = TestModuleCatalogSettingNames.MaxScenariosPerTick;

    /// <summary>Otomatik tetikleyicilerin kullandigi mantiksal ortam anahtari ayar adidir.</summary>
    public const string AutomationEnvironmentKey = TestModuleRunSettingNames.AutomationEnvironmentKey;

    /// <summary>Gelen webhook cagrilarinin dogrulandigi paylasilan sir ayar adidir.</summary>
    public const string WebhookSecret = TestModuleRunSettingNames.WebhookSecret;
}
