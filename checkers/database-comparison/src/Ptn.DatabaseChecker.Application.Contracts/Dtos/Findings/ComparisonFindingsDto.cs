using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Bir run'in tum yapisal/migration/veri bulgularini tek cevap koke toplar.
// sistemdeki gorevi: ComparisonFindings modelinin cevap karsiligi; ComparisonRunDetailDto.Findings olarak dondurulur.
public class ComparisonFindingsDto
{
    /// <summary>
    /// Yapi farklari (tablo/kolon/index/view/trigger...).
    /// </summary>
    public List<SchemaDifferenceDto> SchemaDifferences { get; set; } = new();

    /// <summary>
    /// Migration defter farklari.
    /// </summary>
    public List<MigrationDifferenceDto> MigrationDifferences { get; set; } = new();

    /// <summary>
    /// DataCompare kapsamindaki tablolarin veri farki ozetleri.
    /// </summary>
    public List<DataDifferenceDto> DataDifferences { get; set; } = new();
}
