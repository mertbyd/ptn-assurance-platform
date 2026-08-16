namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Hedef veritabanindaki __EFMigrationsHistory defterinin tek satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: T7 migration karsilastirmasi bu satiri katalog context uzerinden LINQ ile okur (ham SQL yok); iki motor da ayni sekli paylasir, sema farki (public/dbo) provider konfigurasyonunda verilir. Salt-okunur katalog modelidir, app modeline girmez.
public sealed class EfMigrationsHistoryCatalogRow
{
    // Migration'in tam kimligi; __EFMigrationsHistory tablosunun PK'sidir, EF tarafinda zaman damgali ad oldugu icin alfabetik sira kronolojik sira ile uyumludur.
    public string MigrationId { get; set; } = string.Empty;

    // Migration'i uygulayan EF Core surumu; elle olusturulmus eksik kayitlarda null gelebilir.
    public string? ProductVersion { get; set; }
}
