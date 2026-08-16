namespace Ptn.ApiContractChecker.Constants;

// islevi: BusinessException.WithData ile istemciye tasinan kararli metadata anahtarlarini tanimlar.
// sistemdeki gorevi: Seed, auth ve generic manager hata cevaplarinin ayni veri sozlesmesini magic string kullanmadan korumasini saglar.
public static class BusinessExceptionDataKeys
{
    // Alt operasyonlardan toplanan hata aciklamalari.
    public const string Errors = "Errors";

    // Birden cok Identity hatasini tek exception data degerinde ayirir.
    public const string ErrorDescriptionSeparator = "; ";

    // Eksik veya gecersiz configuration anahtari.
    public const string ConfigurationKey = "ConfigurationKey";

    // Tekillik ihlaline neden olan deger.
    public const string Value = "Value";

    // Is kurali ihlaline neden olan alanin kararli adi.
    public const string Field = "Field";

    // Atanmasina izin verilmeyen rol.
    public const string Role = "Role";

    // Gecersiz runtime esiginin setting adi.
    public const string SettingName = "SettingName";
}
