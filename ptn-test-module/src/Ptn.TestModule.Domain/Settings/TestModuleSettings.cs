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

    /// <summary>Tenant-scoped test ortami baglama haritasi ayar adidir.</summary>
    public const string EnvironmentBindings = TestModuleRunSettingNames.EnvironmentBindings;
}
