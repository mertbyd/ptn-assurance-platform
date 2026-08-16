using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.foreign_keys katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasinda FK kisitlamalarinin ad, kaynak/hedef tablo ve cascade davranisini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerForeignKeyCatalogRow : CatalogRowBase<int>
{
    // sys.foreign_keys.parent_object_id: FK'nin ait oldugu kaynak tablo kimligi.
    public int ParentObjectId { get; set; }

    // sys.foreign_keys.referenced_object_id: FK'nin referans aldigi hedef tablo kimligi.
    public int ReferencedObjectId { get; set; }

    // sys.foreign_keys.delete_referential_action: ON DELETE davranisi (0=NO ACTION, 1=CASCADE, 2=SET NULL, 3=SET DEFAULT).
    public byte DeleteReferentialAction { get; set; }

    // sys.foreign_keys.update_referential_action: ON UPDATE davranisi.
    public byte UpdateReferentialAction { get; set; }

    // sys.foreign_keys.is_disabled: FK enforcement devre disi mi.
    public bool IsDisabled { get; set; }

    // sys.foreign_keys.is_not_trusted: mevcut veri FK'ye karsi dogrulanmamis mi.
    public bool IsNotTrusted { get; set; }
}
