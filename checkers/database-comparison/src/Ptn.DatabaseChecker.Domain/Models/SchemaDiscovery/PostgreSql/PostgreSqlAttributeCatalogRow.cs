namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_attribute katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasi icin tablolardaki kolonlari, tiplerini ve null durumlarini getiren salt-okunur modeldir.
public sealed class PostgreSqlAttributeCatalogRow
{
    // Ait oldugu tablonun kimligi (pg_class.oid).
    public uint RelationId { get; set; }

    // Kolon adi.
    public string Name { get; set; } = string.Empty;

    // Kolonun veri tip kimligi (pg_type.oid).
    public uint TypeId { get; set; }

    // Kolon sirasi (> 0 kullanici kolonu).
    public short ColumnNumber { get; set; }

    // NOT NULL kisitlamasi var mi.
    public bool IsNotNull { get; set; }

    // Default degeri var mi.
    public bool HasDefault { get; set; }

    // Kolon silinmis mi (DROP COLUMN sonrasi).
    public bool IsDropped { get; set; }

    // Tip-e-ozel uzunluk/hassasiyet/olcek bilgisi; varchar icin (atttypmod - 4) = MaxLength, numeric icin precision+scale decode kaynagi.
    public int TypeModifier { get; set; }

    // Identity kolon turu ('a' = always, 'd' = by default, '' = degil); PG 10+ identity syntax'i.
    public string Identity { get; set; } = string.Empty;

    // Generated kolon turu ('s' = stored, '' = degil).
    public string Generated { get; set; } = string.Empty;

    // Kolon collation kimligi; collatable olmayan tiplerde 0 olabilir.
    public uint CollationId { get; set; }
}
