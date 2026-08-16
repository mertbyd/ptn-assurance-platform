namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Hedef veritabanindaki tek bir __EFMigrationsHistory kaydini temsil eder.
// sistemdeki gorevi: Provider repository'leri migration defterini bu motor-bagimsiz modele indirger; manager sema, MigrationId ve ProductVersion kararini gorur.
public class MigrationHistoryEntryModel
{
    // Migration history tablosunun bulundugu provider semasi; raporda gercek defter adresini gostermek icin finding'e tasinir.
    public string SchemaName { get; set; } = default!;

    // Migration'in tam kimligi; EF Core tarafinda zaman damgali ad oldugu icin alfabetik sira kronolojik sira ile uyumludur.
    public string MigrationId { get; set; } = default!;

    // Migration'i uygulayan EF Core surumu; elle olusturulmus eksik kayitlarda null gelebilir.
    public string? ProductVersion { get; set; }
}
