using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Secili bir tablonun exact data-comparison icin metadata ve satir fotografini tasir.
// sistemdeki gorevi: Infrastructure JSON okumasini saf domain TableDataComparisonManager girdisine cevirir; persistence modeli degildir.
public class TableDataSnapshotModel
{
    // Fotograflanan tablonun sema adi.
    public string SchemaName { get; set; } = default!;

    // Fotograflanan tablonun adi.
    public string TableName { get; set; } = default!;

    // Exact COUNT(*) sonucu; Rows listesi row-limit icinde ayni sayida eleman tasir.
    public long RowCount { get; set; }

    // Tablodaki kullanici kolonlari.
    public List<string> ColumnNames { get; set; } = new();

    // Composite primary key kolonlari; PK yoksa satir-hash eslestirmesine dusulur.
    public List<string> PrimaryKeyColumns { get; set; } = new();

    // Exact kiyaslamaya alinacak kanonik satirlar.
    public List<TableDataRowModel> Rows { get; set; } = new();
}
