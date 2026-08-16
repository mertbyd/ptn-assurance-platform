using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.default_constraints katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Kolon default degerlerinin SQL ifadesini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerDefaultConstraintCatalogRow : CatalogRowBase<int>
{
    // sys.default_constraints.parent_object_id: ait oldugu tablo kimligi.
    public int ParentObjectId { get; set; }

    // sys.default_constraints.parent_column_id: ait oldugu kolon kimligi.
    public int ParentColumnId { get; set; }

    // sys.default_constraints.definition: default deger SQL ifadesi.
    public string Definition { get; set; } = string.Empty;
}
