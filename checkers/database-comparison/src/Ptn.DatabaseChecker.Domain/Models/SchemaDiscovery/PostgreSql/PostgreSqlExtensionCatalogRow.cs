using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_extension katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Extension adi, namespace'i ve versiyonunu snapshot icindeki tablo disi nesne listesine tasir.
public sealed class PostgreSqlExtensionCatalogRow : CatalogRowBase<uint>
{
    // pg_extension.extnamespace: extension'in kurulu oldugu namespace kimligi.
    public uint NamespaceId { get; set; }

    // pg_extension.extversion: kurulu extension versiyonu.
    public string Version { get; set; } = string.Empty;
}
