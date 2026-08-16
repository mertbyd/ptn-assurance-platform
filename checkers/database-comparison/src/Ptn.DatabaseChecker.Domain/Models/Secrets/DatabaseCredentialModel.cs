namespace Ptn.DatabaseChecker.Models.Secrets;

// islevi: Vault'tan cozulmus DB kimlik bilgisini tasiyan domain modeli.
// sistemdeki gorevi: ISecretProvider giris/cikisinda kullanicin adi + parola cifti; DB'ye asla yazilmaz, yalniz bellekte connection string kurmak icin kullanilir.
public class DatabaseCredentialModel
{
    // DB read-only kullanici adi.
    public string Username { get; set; } = default!;

    // DB parolasi (yalniz bellekte; loglanmaz, DTO'ya konmaz).
    public string Password { get; set; } = default!;
}
