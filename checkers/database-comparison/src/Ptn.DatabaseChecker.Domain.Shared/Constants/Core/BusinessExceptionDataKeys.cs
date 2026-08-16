namespace Ptn.DatabaseChecker.Constants;

// islevi: BusinessException.WithData ile istemciye tasinan kararli metadata anahtarlarini tanimlar.
// sistemdeki gorevi: Seed, auth ve generic manager hata cevaplarinin ayni veri sozlesmesini magic string kullanmadan korumasini saglar.
public static class BusinessExceptionDataKeys
{
    // Alt operasyonlardan toplanan hata aciklamalari.
    public const string Errors = "Errors";

    // Eksik veya gecersiz configuration anahtari.
    public const string ConfigurationKey = "ConfigurationKey";

    // Tekillik ihlaline neden olan deger.
    public const string Value = "Value";

    // Atanmasina izin verilmeyen rol.
    public const string Role = "Role";
}
