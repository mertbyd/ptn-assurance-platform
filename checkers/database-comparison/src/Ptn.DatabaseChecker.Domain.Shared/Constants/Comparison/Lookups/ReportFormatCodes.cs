namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Rapor formati lookup satirlarinin kararli Code degerlerini sabitler.
// sistemdeki gorevi: Seed, FK cozumleme ve rapor uretim/gonderim mantigi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu bu sabitler devralir.
public static class ReportFormatCodes
{
    // Mail govdesine gomulen zengin HTML.
    public const string Html = "Html";

    // Wiki'ye / PR yorumuna yapistirilan Markdown.
    public const string Markdown = "Markdown";

    // islevi: Rapor format kodunun generator tarafindan desteklenip desteklenmedigini kontrol eder.
    // sistemdeki gorevi: API filtresi, persisted-content fallback ve mail akisinin ayni kararli kod kumesini kullanmasini saglar.
    public static bool IsSupported(string? code)
        => code == Html || code == Markdown;
}
