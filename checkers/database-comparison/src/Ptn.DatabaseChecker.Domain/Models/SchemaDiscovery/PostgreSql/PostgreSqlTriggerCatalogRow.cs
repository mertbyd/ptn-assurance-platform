using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_trigger katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kesfinde external veritabanindaki trigger adlarini ve bagli relation kimligini tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlTriggerCatalogRow : CatalogRowBase<uint>
{
    // pg_trigger.tgrelid: bagli tablo/view kimligi.
    public uint RelationId { get; set; }

    // pg_trigger.tgisinternal: sistem trigger'i mi.
    public bool IsInternal { get; set; }

    // pg_trigger.tgenabled: origin/replica/always/disabled durum kodu.
    public string EnabledStatus { get; set; } = string.Empty;
}
