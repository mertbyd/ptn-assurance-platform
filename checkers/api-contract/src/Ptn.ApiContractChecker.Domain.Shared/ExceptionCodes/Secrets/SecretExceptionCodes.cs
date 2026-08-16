namespace Ptn.ApiContractChecker.ExceptionCodes;

// islevi: Secret deposu (Vault) okuma islemlerinin is-kurali hata kodlarini tanimlar.
// sistemdeki gorevi: ISecretProvider implementasyonu, ham VaultApiException yerine bu kodlarla anlamli BusinessException firlatir; test-connection ve run execution jenerik 500 yerine lokalize mesaj gosterir.
public static class SecretExceptionCodes
{
    private const string Prefix = "ApiContractChecker.Secret";

    // Baglantinin Vault kimlik bilgisi bulunamadiginda firlatilir (secret hic yazilmamis veya silinmis).
    public const string NotFound = $"{Prefix}:SecretNotFound";
}
