namespace Ptn.TestModule.Constants;

// islevi: Test Module'un kararli veritabani adlarini tanimlar.
// sistemdeki gorevi: Domain.Shared disinda sema, baglanti ve migration history literal'i kalmasini engeller (ADR-0016 §A, RULE-0002).
public static class TestModuleDatabaseConstants
{
    public const string EmptyTablePrefix = "";
    public const string ConnectionStringName = "TestModule";
    public const string DefaultConnectionStringName = "Default";
    public const string MigrationsHistoryTableName = "__test_module_migrations_history";

    // Sabit deger listeleri (seed). Kucuk, global, nadiren degisir.
    public const string LookupSchema = "test_lookup";

    // Tanim dunyasi: senaryo, ortam, plan, is bilgisi, ajan oturumu. Uzun omurlu.
    public const string CatalogSchema = "test_catalog";

    // Kosum dunyasi: kosu ve kosum kayitlari. Buyuk, 90 gun saklanir.
    public const string RunSchema = "test_run";
}
