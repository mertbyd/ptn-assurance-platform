namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir baglantidaki tek bir kullanici semasinin hafif kesif temsilidir.
// sistemdeki gorevi: T4 sema listeleme akisinda motor-bagimsiz sema adini tasir; kullanici once bu listeden secer, sonra derin snapshot yalniz secilen semaya daraltilir.
public class DatabaseSchemaModel
{
    // Kullanici semasinin adi (PG: nspname, MSSQL: sys.schemas.name); sistem/rol semalari okuyucuda elenir.
    public string Name { get; set; } = string.Empty;
}
