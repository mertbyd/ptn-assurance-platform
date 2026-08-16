namespace Ptn.DatabaseChecker.Models.Comparison.Reports;

// islevi: Run bitince bulgulardan uretilen insan-okur dokumanin tek bir formattaki hali (Html veya Markdown); entity DEGIL, ComparisonRun'a owned JSON olarak gomulur.
// sistemdeki gorevi: Eski ComparisonReport tablosunun owned-JSON karsiligi; mailin kaniti ve "manuel yeniden gonder" icin uretildigi anda donar. Format basina bir eleman (Html mail'e, Markdown wiki'ye).
public class ComparisonReportContent
{
    // Rapor formatinin kararli kodu (ReportFormatCodes.*): Html / Markdown.
    public string FormatCode { get; set; } = default!;

    // Raporun tam metni; uzun olabilir (yuzlerce KB). Run listesi bu metni tasimasin diye liste sorgusu owned kolonlari projekte etmemeli.
    public string Content { get; set; } = default!;
}
