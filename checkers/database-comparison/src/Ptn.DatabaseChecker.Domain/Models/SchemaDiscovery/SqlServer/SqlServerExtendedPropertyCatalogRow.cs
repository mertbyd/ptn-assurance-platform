namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.extended_properties katalog satirinin scope, ad ve sql_variant degerini temsil eder.
// sistemdeki gorevi: Kolon seviyesindeki MS_Description degerini object_id + column_id anahtariyla snapshot'a tasir.
public sealed class SqlServerExtendedPropertyCatalogRow
{
    public int Class { get; set; }
    public int MajorId { get; set; }
    public int MinorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
}
