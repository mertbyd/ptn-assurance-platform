using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_collation katalog satirinin ad ve provider kodunu temsil eder.
// sistemdeki gorevi: Kolon collation adlarini ve bulunabilirse veritabani collation provider kodunu snapshot'a tasir.
public sealed class PostgreSqlCollationCatalogRow : CatalogRowBase<uint>
{
    // pg_collation.collprovider: libc/ICU/builtin provider kodu.
    public string ProviderCode { get; set; } = string.Empty;
}
