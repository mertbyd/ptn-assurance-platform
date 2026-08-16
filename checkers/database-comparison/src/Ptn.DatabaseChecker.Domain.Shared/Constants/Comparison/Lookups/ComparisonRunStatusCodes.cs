namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Calistirma durumu lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve motor yasam-dongusu mantigi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class ComparisonRunStatusCodes
{
    // Kuyruga girdi, motor henuz baslamadi (StartedAt null).
    public const string Pending = "Pending";

    // Motor fiilen calisiyor (StartedAt dolu, CompletedAt null).
    public const string Running = "Running";

    // Basariyla bitti (CompletedAt dolu, sayaclar yazildi).
    public const string Completed = "Completed";

    // Calisamadi; sebep ErrorMessage'ta ("fark yok" ile karistirilmaz).
    public const string Failed = "Failed";
}
