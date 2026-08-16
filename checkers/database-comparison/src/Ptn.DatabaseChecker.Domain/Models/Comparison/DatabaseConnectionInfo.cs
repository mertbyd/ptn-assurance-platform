using Ptn.DatabaseChecker.Models.Connections;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir hedef veritabanina baglanmak icin gereken calisma-zamani bilgisini tasir (kimlik dahil); entity DEGIL, hicbir yere persist edilmez.
// sistemdeki gorevi: AppService entity alanlari + Vault'tan cozulen kimlikten bunu kurar, connection tester connection string'i bundan uretir. Sifre yalnizca bellekte yasar.
public class DatabaseConnectionInfo
{
    // Sunucu adresi (host adi veya IP).
    public string Host { get; set; } = default!;

    // Sunucu portu.
    public int Port { get; set; }

    // Hedef veritabani adi.
    public string DatabaseName { get; set; } = default!;

    // Vault'tan cozulen kullanici adi.
    public string Username { get; set; } = default!;

    // Vault'tan cozulen sifre; yalnizca connection string kurmak icin bellekte tutulur.
    public string Password { get; set; } = default!;

    // Ayar zinciri ve entity alanlarindan tek noktada cozulen hedef baglanti emniyeti profili.
    public ConnectionSafetyProfile SafetyProfile { get; set; } = default!;
}
