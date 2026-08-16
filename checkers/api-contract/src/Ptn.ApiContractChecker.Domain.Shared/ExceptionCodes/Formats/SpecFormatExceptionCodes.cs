namespace Ptn.ApiContractChecker.ExceptionCodes;

// islevi: Spec formati cozumleme hatalarinin kararli kodlarini tutar.
// sistemdeki gorevi: Format-ozel bilesen bulunamadiginda kullaniciya donen kodu tek yerde tanimlar; mesaj metni localization'dan gelir.
public static class SpecFormatExceptionCodes
{
    private const string Prefix = "ApiContractChecker.SpecFormat";

    // Verilen format koduna kayitli bir bilesen yok (desteklenmeyen spec surumu).
    public const string UnsupportedFormat = $"{Prefix}:00001";
}
