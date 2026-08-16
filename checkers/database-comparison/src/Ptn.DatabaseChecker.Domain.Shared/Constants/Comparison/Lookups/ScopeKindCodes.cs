namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Kapsam kurali lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve motor kapsam mantigi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class ScopeKindCodes
{
    // "Sema karsilastirmasina bunlar girsin" (beyaz liste).
    public const string Include = "Include";

    // "Bunlara bakma" (kara liste / ignore); cakismada Exclude kazanir.
    public const string Exclude = "Exclude";

    // "Gecici/log/teknik nesneleri yok say"; motor tarafinda Exclude ile ayni bloklayici etkiyi tasir, UI'da ayri listelenir.
    public const string Ignore = "Ignore";

    // "Veri karsilastirmasi (COUNT/hash) sadece bunlarda" (canliyi yormamak icin).
    public const string DataCompare = "DataCompare";
}
