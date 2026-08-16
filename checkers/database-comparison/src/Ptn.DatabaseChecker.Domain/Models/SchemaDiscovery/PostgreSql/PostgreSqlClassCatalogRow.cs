using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_class katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki relation (tablo/view/sequence) adlarini ve relkind turunu tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlClassCatalogRow : CatalogRowBase<uint>
{
    // pg_class.relkind: nesne turu kodu.
    public string Kind { get; set; } = string.Empty;

    // pg_class.relnamespace: sema kimligi.
    public uint NamespaceId { get; set; }
}
