namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_depend katalog satirinin nesne ve referans kimliklerini temsil eder.
// sistemdeki gorevi: Identity kolonun sahip oldugu sequence'i tablo/kolon numarasi uzerinden pg_sequence'e baglar.
public sealed class PostgreSqlDependCatalogRow
{
    public uint CatalogId { get; set; }
    public uint ObjectId { get; set; }
    public int ObjectSubId { get; set; }
    public uint ReferencedCatalogId { get; set; }
    public uint ReferencedObjectId { get; set; }
    public int ReferencedObjectSubId { get; set; }
    public string DependencyType { get; set; } = string.Empty;
}
