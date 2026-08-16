namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.index_columns katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Indekslerin anahtar ve INCLUDE kolonlarini sirasiyla cozumleyen salt-okunur katalog modelidir.
public sealed class SqlServerIndexColumnCatalogRow
{
    // sys.index_columns.object_id: ait oldugu tablo kimligi.
    public int ObjectId { get; set; }

    // sys.index_columns.index_id: ait oldugu indeks kimligi.
    public int IndexId { get; set; }

    // sys.index_columns.column_id: kolon kimligi (sys.columns.column_id referansi).
    public int ColumnId { get; set; }

    // sys.index_columns.key_ordinal: anahtar icindeki sira numarasi; 0 = INCLUDE kolonu.
    public byte KeyOrdinal { get; set; }

    // sys.index_columns.is_included_column: INCLUDE kolonu mu.
    public bool IsIncludedColumn { get; set; }
}
