namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.identity_columns katalog satirinin seed ve increment alanlarini temsil eder.
// sistemdeki gorevi: sql_variant identity degerlerini materialize edip hassasiyet kaybetmeyen kanonik metin kolon alanlarina donusturur.
public sealed class SqlServerIdentityColumnCatalogRow
{
    public int ObjectId { get; set; }
    public int ColumnId { get; set; }
    public object? SeedValue { get; set; }
    public object? IncrementValue { get; set; }
}
