namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_description katalog satirinin nesne, alt-nesne ve aciklama alanlarini temsil eder.
// sistemdeki gorevi: Tablo kolon comment'lerini relation oid + attnum anahtariyla snapshot kolonuna tasir.
public sealed class PostgreSqlDescriptionCatalogRow
{
    public uint ObjectId { get; set; }
    public uint CatalogId { get; set; }
    public int SubObjectId { get; set; }
    public string Description { get; set; } = string.Empty;
}
