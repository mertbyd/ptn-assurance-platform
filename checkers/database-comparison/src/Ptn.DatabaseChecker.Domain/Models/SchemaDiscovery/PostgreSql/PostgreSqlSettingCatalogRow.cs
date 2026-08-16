namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_settings katalogundaki tek setting adi ve etkin degerini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: ServerSettingProbe'un ham SQL veya DbSet eklemeden LINQ ile sinirli session/server olgusu okumasini saglar.
public sealed class PostgreSqlSettingCatalogRow
{
    public string Name { get; set; } = string.Empty;
    public string Setting { get; set; } = string.Empty;
}
