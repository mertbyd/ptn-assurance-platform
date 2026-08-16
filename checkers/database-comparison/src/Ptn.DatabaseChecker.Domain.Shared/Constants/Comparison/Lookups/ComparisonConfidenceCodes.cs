namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Fark guveni lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve rapor durustluk mekanizmasi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class ComparisonConfidenceCodes
{
    // Ayni motor, birebir tip kiyasi; tam guven.
    public const string Exact = "Exact";

    // Capraz motor, kanonik modele cevrilerek kiyas; yuksek guven.
    public const string Canonical = "Canonical";

    // Capraz motor, yaklasik tip esleme; dikkatli yorumlanmali.
    public const string Approximate = "Approximate";

    // Karsiligi olmayan yapi (PG extension, MSSQL sql_variant); makine kiyaslayamaz, insan baksin.
    public const string Incomparable = "Incomparable";
}
