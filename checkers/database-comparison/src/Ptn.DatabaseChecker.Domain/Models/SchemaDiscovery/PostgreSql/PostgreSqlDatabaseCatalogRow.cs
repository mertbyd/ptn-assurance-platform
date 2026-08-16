using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_database katalog satirinin ad ve varsayilan collation alanlarini temsil eder.
// sistemdeki gorevi: Snapshot basligina veritabaninin gercek datcollate degerini ham SQL kullanmadan tasir.
public sealed class PostgreSqlDatabaseCatalogRow : CatalogRowBase<uint>
{
    // pg_database.datcollate: veritabaninin varsayilan collation/locale adi.
    public string CollationName { get; set; } = string.Empty;
}
