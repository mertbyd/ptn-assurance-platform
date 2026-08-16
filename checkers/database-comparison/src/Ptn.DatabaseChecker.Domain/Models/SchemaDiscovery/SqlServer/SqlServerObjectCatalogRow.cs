using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.objects katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki nesne (tablo/view/procedure/function) ad ve turlerini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerObjectCatalogRow : CatalogRowBase<int>
{
    // sys.objects.schema_id: sema kimligi.
    public int SchemaId { get; set; }

    // sys.objects.type: nesne turu kodu.
    public string Type { get; set; } = string.Empty;

    // sys.objects.is_ms_shipped: sistem nesnesi mi.
    public bool IsMsShipped { get; set; }
}
