using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.schemas katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki kullanici semalarinin ad ve schema_id bilgisini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerSchemaCatalogRow : CatalogRowBase<int>
{
}
