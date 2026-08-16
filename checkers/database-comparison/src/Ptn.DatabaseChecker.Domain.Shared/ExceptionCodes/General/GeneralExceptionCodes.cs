namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: Modulden bagimsiz genel is hata kodlarini merkezi tutar.
// sistemdeki gorevi: Manager, repository ve altyapi akislari raw hata kodu string'i yazmadan ortak kodlari kullanir.
public static class GeneralExceptionCodes
{
    // Genel sistem hata kodlarinin ortak prefix'i.
    private const string Prefix = "DatabaseChecker.General";

    // Beklenmeyen veya desteklenmeyen genel islem hatasi.
    public const string InvalidOperation = $"{Prefix}:00001";

    // Yetkisiz islem hata kodu.
    public const string NotAuthorized = $"{Prefix}:00002";

    // ABP Setting veya enum allowlist disinda kalan enum degeri hata kodu.
    public const string InvalidEnumValue = $"{Prefix}:00003";

    // Soft delete desteklemeyen entity icin silme denemesi hata kodu.
    public const string SoftDeleteNotSupported = $"{Prefix}:00004";

    // Guncelleme desteklemeyen manager akisi hata kodu.
    public const string UpdateNotSupported = $"{Prefix}:00005";

    // Silme desteklemeyen manager akisi hata kodu.
    public const string DeleteNotSupported = $"{Prefix}:00006";

    // Elasticsearch okuma veya zorunlu index bakim akisi kullanilamazsa donen hata kodu.
    public const string SearchUnavailable = $"{Prefix}:00007";

    // Verilen kimlikte kayit bulunamadiginda firlatilan modulden bagimsiz genel hata kodu.
    public const string NotFound = $"{Prefix}:00008";
}
