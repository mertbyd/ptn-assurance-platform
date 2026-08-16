namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.computed_columns katalog satirinin ifade ve persisted alanlarini temsil eder.
// sistemdeki gorevi: Computed kolon semantigini sys.columns kolonundan ayri katalog join'iyle snapshot'a tasir.
public sealed class SqlServerComputedColumnCatalogRow
{
    public int ObjectId { get; set; }
    public int ColumnId { get; set; }
    public string? Definition { get; set; }
    public bool IsPersisted { get; set; }
}
