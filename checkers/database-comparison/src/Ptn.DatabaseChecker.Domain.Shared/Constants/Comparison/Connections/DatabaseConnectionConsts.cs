namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: DatabaseConnection kolonlarinin uzunluk sinirlarini tek kaynakta tanimlar.
// sistemdeki gorevi: EF mapping (HasMaxLength) ve FluentValidation (MaximumLength) ayni sabiti kullanir; sema ile validasyon birbirinden kaymaz.
public static class DatabaseConnectionConsts
{
    // Insan-okur baglanti takma adinin azami uzunlugu ("Canli-PG").
    public const int MaxNameLength = 128;

    // Sunucu adresinin (host adi veya IP) azami uzunlugu.
    public const int MaxHostLength = 256;

    // Sunucudaki veritabani adinin azami uzunlugu.
    public const int MaxDatabaseNameLength = 128;

    // Sifrenin Vault'taki adresinin (secret path) azami uzunlugu; sifrenin kendisi asla DB'de tutulmaz.
    public const int MaxVaultSecretPathLength = 512;

    // Kararli TLS politika kodunun azami kolon/girdi uzunlugu.
    public const int MaxTlsModeCodeLength = 16;

    // Kimlik bilgisi girdi sinirlari; bunlar DB'de tutulmaz, Vault'a yazilir - sinir yalnizca girdi sagligi icindir.
    public const int MaxUsernameLength = 256;
    public const int MaxPasswordLength = 256;

    // TCP port araligi; validasyon bu sinirlari kullanir.
    public const int MinPort = 1;
    public const int MaxPort = 65535;
}
