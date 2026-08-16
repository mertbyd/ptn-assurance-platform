using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.types katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Kolon tiplerini cozumlemek ve kullanici tanimli tipleri snapshot'a almak icin user_type_id uzerinden join edilen salt-okunur katalog modelidir.
public sealed class SqlServerTypeCatalogRow : CatalogRowBase<int>
{
    // sys.types.schema_id: tipin ait oldugu sema kimligi.
    public int SchemaId { get; set; }

    // sys.types.system_type_id: sistem tipi kimligi. sys.types'ta kolon tinyint'tir; CLR tipi byte olmalidir
    // yoksa EF materializer GetInt32 cagirir ve tinyint (Byte) degerde InvalidCastException firlatir.
    public byte SystemTypeId { get; set; }

    // sys.types.max_length: tipin varsayilan azami uzunlugu.
    public short MaxLength { get; set; }

    // sys.types.precision: tipin varsayilan hassasiyeti.
    public byte Precision { get; set; }

    // sys.types.scale: tipin varsayilan olcegi.
    public byte Scale { get; set; }

    // sys.types.is_user_defined: kullanici tanimli alias/table type mi.
    public bool IsUserDefined { get; set; }

    // sys.types.is_table_type: tablo tipi mi; alias tiplerden ayirmak icin rapor tanimina girer.
    public bool IsTableType { get; set; }
}
