using Ptn.TestModule.Constants.Bridge;

namespace Ptn.TestModule.Settings;

// islevi: Test Module ayar adlarini mevcut ABP setting provider yuzeyine aktarir.
// sistemdeki gorevi: Domain.Shared sahibi olan kopru ayarini mevcut modul ayar grubuyla uyumlu tutar.
public static class TestModuleSettings
{
    public const string GroupName = "TestModule";
    public const string ProfilePackPath = PtnBridgeSettingNames.ProfilePackPath;
}
