using Ptn.TestModule.Constants.Bridge;
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

    /// <summary>Asili Running kosularinin kurtarilma esigi ayar adidir.</summary>
    public const string StaleRunThresholdMinutes = TestModuleRunSettingNames.StaleRunThresholdMinutes;

    /// <summary>Sandbox verisini kosumdan once temizleyen strateji ayar adidir.</summary>
    public const string SandboxResetStrategy = TestModuleRunSettingNames.SandboxResetStrategy;
}
