using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.databases katalog satirinin ad ve collation alanlarini temsil eder.
// sistemdeki gorevi: Snapshot basligina hedef veritabaninin varsayilan collation adini LINQ ile tasir.
public sealed class SqlServerDatabaseCatalogRow : CatalogRowBase<int>
{
    public string? CollationName { get; set; }
}
