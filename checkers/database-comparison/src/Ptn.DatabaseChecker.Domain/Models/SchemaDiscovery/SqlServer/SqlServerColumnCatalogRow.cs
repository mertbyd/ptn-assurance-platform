namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.columns katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sema kiyaslamasinda tablolardaki kolon adi, sirasi, tipi, null durumu ve identity bilgisini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerColumnCatalogRow
{
    // sys.columns.object_id: ait oldugu tablo kimligi.
    public int ObjectId { get; set; }

    // sys.columns.name: kolon adi.
    public string Name { get; set; } = string.Empty;

    // sys.columns.column_id: tablo icinde benzersiz sira numarasi.
    public int ColumnId { get; set; }

    // sys.columns.system_type_id: sistem veri tipi kimligi. SQL Server'da tinyint oldugu icin CLR karsiligi byte'tir; int okunursa EF materialization "Byte -> Int32" cast hatasi verir.
    public byte SystemTypeId { get; set; }

    // sys.columns.user_type_id: kullanici tanimli veri tipi kimligi.
    public int UserTypeId { get; set; }

    // sys.columns.max_length: kolon azami uzunlugu (byte cinsinden; nvarchar icin ikiye bolunur).
    public short MaxLength { get; set; }

    // sys.columns.precision: sayisal hassasiyet.
    public byte Precision { get; set; }

    // sys.columns.scale: sayisal olcek.
    public byte Scale { get; set; }

    // sys.columns.is_nullable: NULL kabul eder mi.
    public bool IsNullable { get; set; }

    // sys.columns.is_identity: identity kolonu mu.
    public bool IsIdentity { get; set; }

    // sys.columns.is_computed: hesaplanmis kolon mu.
    public bool IsComputed { get; set; }

    // sys.columns.collation_name: collatable kolonun collation adi; diger tiplerde null.
    public string? CollationName { get; set; }
}
