using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_constraint katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasinda tablolardaki FK, PK, unique ve check kisitlamalarinin ad, tur ve hedef bilgisini tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlConstraintCatalogRow : CatalogRowBase<uint>
{
    // pg_constraint.contype: kisitlama turu kodu ('f' = FK, 'p' = PK, 'u' = unique, 'c' = check).
    public string Type { get; set; } = string.Empty;

    // pg_constraint.conrelid: kisitlamanin ait oldugu tablo kimligi.
    public uint TableRelId { get; set; }

    // pg_constraint.confrelid: FK ise referans aldigi tablo kimligi; FK disinda 0.
    public uint ForeignTableRelId { get; set; }

    // pg_constraint.conkey: kaynak kolon numaralari; check constraint'lerde null/empty olabilir.
    public short[]? ColumnNumbers { get; set; }

    // pg_constraint.confkey: FK hedef kolon numaralari; FK disinda null/empty olur.
    public short[]? ForeignColumnNumbers { get; set; }

    // pg_constraint.confdeltype: FK ON DELETE davranis kodu.
    public string DeleteAction { get; set; } = string.Empty;

    // pg_constraint.confupdtype: FK ON UPDATE davranis kodu.
    public string UpdateAction { get; set; } = string.Empty;

    // pg_constraint.convalidated: mevcut verinin kisita karsi dogrulandigini bildirir.
    public bool IsValidated { get; set; }

    // pg_constraint.condeferrable: kisit transaction sonuna ertelenebilir mi.
    public bool IsDeferrable { get; set; }

    // pg_constraint.condeferred: kisit varsayilan olarak ertelenmis mi.
    public bool IsInitiallyDeferred { get; set; }
}
