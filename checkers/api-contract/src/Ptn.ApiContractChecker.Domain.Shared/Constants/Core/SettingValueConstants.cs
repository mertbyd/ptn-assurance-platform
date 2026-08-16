namespace Ptn.ApiContractChecker.Constants;

// islevi: Kod tarafinda serialize/parse edilen ABP Setting degerlerinin ortak metin formatini tanimlar.
// sistemdeki gorevi: Setting provider ve manager'larin allowlist degerlerini farkli ayiraclarla yorumlamasini engeller.
public static class SettingValueConstants
{
    // Enum allowlist setting degerlerini ayiran token.
    public const char ListSeparator = ',';
    public const string False = "false";
    public const string True = "true";
}
