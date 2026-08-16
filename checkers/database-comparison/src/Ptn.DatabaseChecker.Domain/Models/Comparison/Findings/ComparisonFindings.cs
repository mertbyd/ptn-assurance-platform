using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Bir run'in tum yapisal / migration / veri bulgularini tek owned koke toplar; ComparisonRun'a jsonb kolonu olarak gomulur.
// sistemdeki gorevi: Eski SchemaDifference / MigrationDifference / DataDifference (+ satir/hucre) tablolarinin yerini alir; rapor bu modelden uretilir, Run'daki ozet sayaclar bu listelerin uzunlugundan hesaplanir.
public class ComparisonFindings
{
    // Yapi farklari (tablo/kolon/index/view/trigger...); adres + yon + guven + kanit tanimlari.
    public List<SchemaDifferenceModel> SchemaDifferences { get; set; } = new();

    // __EFMigrationsHistory defter farklari; "neden farkli?" baglami (bekleyen deploy mi, elle mi).
    public List<MigrationDifferenceModel> MigrationDifferences { get; set; } = new();

    // DataCompare kapsamindaki tablolarin veri karsilastirma ozetleri; ic ice satir/hucre farklarini tasir.
    public List<DataDifferenceModel> DataDifferences { get; set; } = new();
}
