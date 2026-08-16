namespace Ptn.DatabaseChecker.Dtos.Reports;

// islevi: Uretilmis raporun tek bir formattaki icerigi (API modeli).
// sistemdeki gorevi: ComparisonReportContent modelinin cevap karsiligi; ComparisonRunDetailDto.Reports icinde format basina bir eleman.
public class ComparisonReportContentDto
{
    /// <summary>
    /// Rapor formatinin kararli kodu (ReportFormatCodes.*): Html / Markdown.
    /// </summary>
    public string FormatCode { get; set; } = default!;

    /// <summary>
    /// Raporun tam metni (uzun olabilir).
    /// </summary>
    public string Content { get; set; } = default!;
}
