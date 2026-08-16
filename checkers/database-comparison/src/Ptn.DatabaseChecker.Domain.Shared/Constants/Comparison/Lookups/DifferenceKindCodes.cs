namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Fark yonu lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve rapor yorumlama mantigi ayni string'e baglanir; SchemaDifference/MigrationDifference/DataRowDifference'in ortak yon sozlugudur (enum'un derleme-zamani rolunu devralir).
public static class DifferenceKindCodes
{
    // Kaynakta var, hedefte yok. (Source=Test, Target=Canli ise: canliya cikmamis, bekleyen deploy.)
    public const string OnlyInSource = "OnlyInSource";

    // Sadece hedefte var. (Canlida elle yapilmis supheli degisiklik.)
    public const string OnlyInTarget = "OnlyInTarget";

    // Iki tarafta da var ama farkli.
    public const string Modified = "Modified";

    /// <summary>Kodun kapali fark yonu katalogunda olup olmadigini bildirir.</summary>
    public static bool IsDefined(string? code)
        => code is OnlyInSource or OnlyInTarget or Modified;
}
