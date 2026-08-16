namespace Ptn.DatabaseChecker.Models.SchemaDiscovery;

// islevi: Katalog satir modellerinin ortak PK + ad alanlarini tek yerde tanimlar.
// sistemdeki gorevi: PostgreSQL (uint oid) ve SQL Server (int object_id) katalog satirlari bu base'den turetilir; Id ve Name tekrari ortadan kalkar, EF konfigurasyonlari kolon adini entity bazinda override eder.
public abstract class CatalogRowBase<TKey> where TKey : struct
{
    // Katalog nesne kimligi; PG'de uint (oid), SS'de int (object_id/schema_id).
    public TKey Id { get; set; }

    // Katalog nesne adi; PG'de nspname/relname/proname, SS'de name.
    public string Name { get; set; } = string.Empty;
}
