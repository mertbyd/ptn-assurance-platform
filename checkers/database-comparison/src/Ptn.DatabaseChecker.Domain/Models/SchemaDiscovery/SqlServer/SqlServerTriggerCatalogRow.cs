using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.triggers katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki trigger ad ve bagli nesne kimligini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerTriggerCatalogRow : CatalogRowBase<int>
{
    // sys.triggers.parent_id: bagli nesne kimligi.
    public int ParentObjectId { get; set; }

    // sys.triggers.is_ms_shipped: sistem trigger'i mi.
    public bool IsMsShipped { get; set; }

    // sys.triggers.is_disabled: trigger devre disi mi.
    public bool IsDisabled { get; set; }
}
