using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: DataCompare icin secilmis mevcut tablonun kolon ve primary-key yapisini motor-bagimsiz tasir.
// sistemdeki gorevi: Provider katalog sorgulari ile tek-batch row JSON okumasini ve saf domain PK eslestirmesini birbirine baglar.
public class TableDataStructureModel
{
    // Mevcut tablonun sema adi.
    public string SchemaName { get; set; } = default!;

    // Mevcut tablonun adi.
    public string TableName { get; set; } = default!;

    // Tablodaki kullanici kolonlari, fiziksel sirasiyla.
    public List<string> ColumnNames { get; set; } = new();

    // Primary key kolonlari, composite-key sirasiyla; PK yoksa bos.
    public List<string> PrimaryKeyColumns { get; set; } = new();

    // Assertion matcher'larinin kullanacagi kanonik kolon tipleri.
    public List<TableDataColumnModel> Columns { get; set; } = new();

    // PK ve filtresiz unique index/constraint kolon kumeleri; her ic liste tek bir benzersiz anahtardir.
    public List<List<string>> UniqueKeyColumnSets { get; set; } = new();
}
