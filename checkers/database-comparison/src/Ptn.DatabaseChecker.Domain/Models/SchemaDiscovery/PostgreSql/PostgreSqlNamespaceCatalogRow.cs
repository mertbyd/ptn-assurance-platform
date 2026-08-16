using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_namespace katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki kullanici semalarinin adlarini tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlNamespaceCatalogRow : CatalogRowBase<uint>
{
}
