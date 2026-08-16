namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Karsilastirma modu lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve motor mantigi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class ComparisonTypeCodes
{
    // Sadece yapi (tablo/kolon/index/view/trigger) + migration kiyasi; veriye bakilmaz. Ucuz, saniyeler.
    public const string SchemaOnly = "SchemaOnly";

    // Sadece veri (COUNT/hash/satir); yapiya bakilmaz. Tabloya gore pahali, canliyi yorabilir.
    public const string DataOnly = "DataOnly";

    // Yapi + veri + migration; gunluk tam saglik kontrolu.
    public const string Both = "Both";

    // islevi: Verilen modun yapisal (sema + migration) karsilastirma icerip icermedigini soyler.
    // sistemdeki gorevi: Motor orkestrasyonu bu kod setinin anlamini tek yerden okur; SchemaOnly/DataOnly/Both branch'i cagri yerlerine dagilmaz (kod setinin anlam otoritesi buradadir).
    public static bool IncludesSchema(string code)
        => code == SchemaOnly || code == Both;

    // islevi: Verilen modun exact satir/hucre veri karsilastirmasi icerip icermedigini soyler.
    // sistemdeki gorevi: DataOnly/Both modlari veri okur; SchemaOnly okumaz. Motor bu ayrimi bu metottan alir.
    public static bool IncludesData(string code)
        => code == DataOnly || code == Both;
}
