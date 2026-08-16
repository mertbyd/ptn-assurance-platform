using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.check_constraints katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Check constraint adini, ait oldugu tabloyu ve ham definition metnini schema snapshot'a tasir.
public sealed class SqlServerCheckConstraintCatalogRow : CatalogRowBase<int>
{
    // sys.check_constraints.parent_object_id: check constraint'in ait oldugu tablo kimligi.
    public int ParentObjectId { get; set; }

    // sys.check_constraints.definition: check expression metni.
    public string Definition { get; set; } = string.Empty;

    // sys.check_constraints.is_ms_shipped: sistem tarafindan olusturulmus mu.
    public bool IsMsShipped { get; set; }

    // sys.check_constraints.is_disabled: check enforcement devre disi mi.
    public bool IsDisabled { get; set; }

    // sys.check_constraints.is_not_trusted: mevcut veri check'e karsi dogrulanmamis mi.
    public bool IsNotTrusted { get; set; }
}
