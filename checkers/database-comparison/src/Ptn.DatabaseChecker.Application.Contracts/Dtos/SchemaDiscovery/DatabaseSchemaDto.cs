namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Bir baglantidaki tek kullanici semasinin API cevap modelidir.
// sistemdeki gorevi: T4 sema listeleme ucunun ciktisi; frontend once semayi secer, ardindan nesne listeleme / snapshot uclarini bu adla cagirir.
public class DatabaseSchemaDto
{
    /// <summary>
    /// Kullanici semasinin adi.
    /// </summary>
    public string Name { get; set; } = default!;
}
