using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_type katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Kolon tipi cozumleme ve kullanici tip/enum kesfi icin gerekli tip adi, namespace ve tur bilgisini tasiyan salt-okunur katalog modelidir.
public sealed class PostgreSqlTypeCatalogRow : CatalogRowBase<uint>
{
    // pg_type.typnamespace: tipin ait oldugu sema kimligi.
    public uint NamespaceId { get; set; }

    // pg_type.typtype: tip tur kodu (enum/domain vb.).
    public string TypeKind { get; set; } = string.Empty;

    // pg_type.typbasetype: domain tipinin temel tip kimligi; domain degilse 0.
    public uint BaseTypeId { get; set; }
}
