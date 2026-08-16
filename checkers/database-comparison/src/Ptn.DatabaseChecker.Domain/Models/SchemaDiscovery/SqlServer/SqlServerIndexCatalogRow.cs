namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.indexes katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasinda tablolardaki indekslerin ad, tekillik, PK durumu ve filtre bilgisini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerIndexCatalogRow
{
    // sys.indexes.object_id: ait oldugu tablo kimligi.
    public int ObjectId { get; set; }

    // sys.indexes.index_id: tablo icinde benzersiz indeks kimligi.
    public int IndexId { get; set; }

    // sys.indexes.name: indeks adi; heap (index_id=0) icin null olabilir.
    public string? Name { get; set; }

    // sys.indexes.is_primary_key: birincil anahtar indeksi mi.
    public bool IsPrimaryKey { get; set; }

    // sys.indexes.is_unique: tekillik kisitlamasi var mi.
    public bool IsUnique { get; set; }

    // sys.indexes.is_unique_constraint: unique constraint mi (index degil).
    public bool IsUniqueConstraint { get; set; }

    // sys.indexes.type: indeks turu (0=heap, 1=clustered, 2=nonclustered).
    public byte IndexType { get; set; }

    // sys.indexes.filter_definition: kosullu indeks WHERE ifadesi; yoksa null.
    public string? FilterDefinition { get; set; }

    // sys.indexes.is_disabled: indeks ve ona bagli PK/unique kisit devre disi mi.
    public bool IsDisabled { get; set; }
}
