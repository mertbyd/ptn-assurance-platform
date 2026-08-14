namespace Ptn.TestModule.Constants.Bridge;

// islevi: Kopru profil paketi konumunun kararli ayar adini ve varsayilan yolunu tanimlar.
// sistemdeki gorevi: Domain, provider ve host katmanlarinin ayni Domain.Shared ayar sozlesmesini kullanmasini saglar.
public static class PtnBridgeSettingNames
{
    public const string ProfilePackPath = "TestModule.Bridge.ProfilePackPath";
    public const string DefaultProfilePackPath = "samples/profiles";
    public const string ProfilePackExtension = ".yaml";
    public const string FingerprintPrefix = "sha256:";
}
