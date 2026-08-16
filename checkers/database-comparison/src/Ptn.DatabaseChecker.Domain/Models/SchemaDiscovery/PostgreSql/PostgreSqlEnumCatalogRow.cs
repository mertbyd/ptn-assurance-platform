using Ptn.DatabaseChecker.Models.SchemaDiscovery;

namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_enum katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Enum tiplerinin sirali etiketlerini okuyup SchemaObjectDefinitionModel tanimina katmak icin kullanilan salt-okunur katalog modelidir.
public sealed class PostgreSqlEnumCatalogRow : CatalogRowBase<uint>
{
    // pg_enum.enumtypid: etiketin ait oldugu enum tip kimligi.
    public uint TypeId { get; set; }

    // pg_enum.enumsortorder: enum etiketinin kararli sirasi.
    public float SortOrder { get; set; }
}
