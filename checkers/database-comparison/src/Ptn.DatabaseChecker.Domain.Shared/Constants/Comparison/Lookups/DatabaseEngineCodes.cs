namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Sema okuyucularinin ve seed'in ortak kullandigi kararli motor kodlarini sabitler.
// sistemdeki gorevi: Lookup satirinin Code alani ile okuyucu sinifinin kimligi ayni string'e baglanir; enum'un derleme-zamani kimlik rolunu (yeni motor = yeni kod + yeni okuyucu) bu sabitler devralir.
public static class DatabaseEngineCodes
{
    // PostgreSQL okuyucusu (Npgsql + pg_catalog, 01 no'lu belge) bu kodla kendini tanitir; seed edilen lookup satirinin Code'u da budur.
    public const string PostgreSql = "PostgreSql";

    // SQL Server okuyucusu (Microsoft.Data.SqlClient + sys.*, 01 no'lu belge) bu kodla kendini tanitir; seed edilen lookup satirinin Code'u da budur.
    public const string SqlServer = "SqlServer";
}
