namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Secili bir tablonun kesin COUNT(*) sonucunu tasir.
// sistemdeki gorevi: Provider repository row-count okumasini manager'in yon/fark kararindan ayirir; tablo adresi ve sayim sonucu tek modelde gelir.
public class TableRowCountModel
{
    // Sayilan tablonun semasi.
    public string SchemaName { get; set; } = default!;

    // Sayilan tablonun adi.
    public string TableName { get; set; } = default!;

    // COUNT(*) / count_big(*) ile alinmis kesin satir sayisi.
    public long RowCount { get; set; }
}
