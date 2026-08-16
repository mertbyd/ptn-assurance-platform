using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Dtos.Reports;

namespace Ptn.DatabaseChecker.Dtos.Runs;

// islevi: Tek bir run'in AGIR detay cevap modelidir; header'a ek olarak bulgular ve rapor icerikleri tasir.
// sistemdeki gorevi: Yalnizca "run detayi/raporu ac" akisinda dondurulur (GetDetailAsync); Run listesi bu owned veriyi tasimaz (ComparisonRunDto hafif kalir).
public class ComparisonRunDetailDto : ComparisonRunDto
{
    /// <summary>
    /// Run'in tum bulgulari (sema/migration/veri).
    /// </summary>
    public ComparisonFindingsDto Findings { get; set; } = new();

    /// <summary>
    /// Run icin uretilmis rapor icerikleri (Html/Markdown).
    /// </summary>
    public List<ComparisonReportContentDto> Reports { get; set; } = new();
}
