using Ptn.TestModule.Constants;

namespace Ptn.TestModule;

// islevi: Test Module tablolarinin yasadigi semalari ve baglanti dizesi adini tutar.
// sistemdeki gorevi: EF configuration dosyalari sema adini buradan okur; ortam bazli override
// EntityFrameworkCore modulunun ConfigureSchemas metodunda yapilir (ADR-0016 §A).
public static class TestModuleDbProperties
{
    public static string DbTablePrefix { get; set; } = TestModuleDatabaseConstants.EmptyTablePrefix;

    // Sabit deger listeleri; seed edilir, global referans verisidir.
    public static string LookupSchema { get; set; } = TestModuleDatabaseConstants.LookupSchema;

    // Tanim dunyasi: senaryo, ortam, plan, is bilgisi, ajan oturumu.
    public static string CatalogSchema { get; set; } = TestModuleDatabaseConstants.CatalogSchema;

    // Kosum dunyasi: kosu ve kosum kayitlari; 90 gun saklanir.
    public static string RunSchema { get; set; } = TestModuleDatabaseConstants.RunSchema;

    public const string ConnectionStringName = TestModuleDatabaseConstants.ConnectionStringName;
}
