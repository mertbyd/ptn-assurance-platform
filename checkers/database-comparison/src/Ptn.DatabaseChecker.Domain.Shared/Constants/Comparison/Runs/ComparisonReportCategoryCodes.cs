namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Rapordaki fark satirlarinin ait oldugu ust kategori kodlarini (Sema / Veri / Migration) sabitler.
// sistemdeki gorevi: Rapor uretici satirlari, rapor render'i bolum basliklari ve veri/migration bulgularinin sozde nesne-turu anahtari ayni string'e baglanir; kategori ekseninin tek kaynagidir (>1 yerde kullanildigi icin sabit).
public static class ComparisonReportCategoryCodes
{
    // Yapi (tablo/kolon/index/view/trigger...) farklari.
    public const string Schema = "Schema";

    // Veri (tablo/satir/hucre) farklari.
    public const string Data = "Data";

    // __EFMigrationsHistory defter farklari.
    public const string Migration = "Migration";
}
