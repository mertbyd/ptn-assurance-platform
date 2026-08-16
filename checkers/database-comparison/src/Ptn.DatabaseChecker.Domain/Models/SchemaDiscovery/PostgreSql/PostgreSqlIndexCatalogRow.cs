namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_index katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasinda tablolardaki indekslerin tekillik, PK durumu ve anahtar kolon sayisini tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlIndexCatalogRow
{
    // pg_index.indexrelid: indeks nesne kimligi (pg_class.oid referansi).
    public uint IndexRelId { get; set; }

    // pg_index.indrelid: indeksin ait oldugu tablo kimligi (pg_class.oid referansi).
    public uint TableRelId { get; set; }

    // pg_index.indisunique: tekillik kisitlamasi var mi.
    public bool IsUnique { get; set; }

    // pg_index.indisprimary: birincil anahtar indeksi mi.
    public bool IsPrimary { get; set; }

    // pg_index.indnkeyatts: anahtar kolon sayisi; INCLUDE kolonlarini ayirt etmek icin kullanilir.
    public short NumberOfKeyColumns { get; set; }

    // pg_index.indkey: index kolon numaralari; ilk NumberOfKeyColumns adet anahtar, kalani INCLUDE kolonudur.
    public short[] ColumnNumbers { get; set; } = [];

    // pg_index.indpred: partial index predicate parse tree degeri; normal indexlerde null.
    public string? PredicateExpression { get; set; }
}
