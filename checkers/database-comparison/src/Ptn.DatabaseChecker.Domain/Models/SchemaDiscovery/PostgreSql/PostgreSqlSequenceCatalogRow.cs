namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_sequence katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sequence ayarlarini pg_class adiyla birlestirip snapshot icindeki tablo disi nesne tanimina tasir.
public sealed class PostgreSqlSequenceCatalogRow
{
    // pg_sequence.seqrelid: sequence'in pg_class oid kimligi.
    public uint SequenceRelId { get; set; }

    // pg_sequence.seqstart: baslangic degeri.
    public long StartValue { get; set; }

    // pg_sequence.seqincrement: artis miktari.
    public long Increment { get; set; }

    // pg_sequence.seqmax: maksimum deger.
    public long MaximumValue { get; set; }

    // pg_sequence.seqmin: minimum deger.
    public long MinimumValue { get; set; }

    // pg_sequence.seqcache: cache degeri.
    public long CacheValue { get; set; }

    // pg_sequence.seqcycle: cycle davranisi.
    public bool IsCycling { get; set; }
}
