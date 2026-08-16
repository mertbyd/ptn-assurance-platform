using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_proc katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki function/procedure adlarini ve prokind turunu tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlProcedureCatalogRow : CatalogRowBase<uint>
{
    // pg_proc.prokind: rutin turu kodu.
    public string Kind { get; set; } = string.Empty;

    // pg_proc.pronamespace: sema kimligi.
    public uint NamespaceId { get; set; }
}
