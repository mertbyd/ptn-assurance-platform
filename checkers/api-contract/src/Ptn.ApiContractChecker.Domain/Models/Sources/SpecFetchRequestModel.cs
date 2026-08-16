using Ptn.ApiContractChecker.Constants;

namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Tek bir canli spec dokumaninin mutlak adresini ve opsiyonel Vault yolunu cekme adapterine tasir.
// sistemdeki gorevi: HTTP adapterinin aggregate veya uygulama DTO'suna baglanmadan request kurabilmesini saglar.
public class SpecFetchRequestModel
{
    // Canli spec govdesinin alinacagi mutlak adres.
    public Uri DocumentUri { get; }

    // Credential gerekiyorsa yalniz istek kurulurken cozulmesi icin kullanilan Vault yolu.
    public string? VaultSecretPath { get; }

    // Kaynak taban adresiyle dokuman yolunu tek kararli mutlak adreste birlestirir.
    public SpecFetchRequestModel(string baseUrl, string documentPath, string? vaultSecretPath)
    {
        var baseUri = new Uri(baseUrl + ApiContractCheckerRoutes.Separator, UriKind.Absolute);
        DocumentUri = new Uri(baseUri, documentPath.TrimStart(ApiContractCheckerRoutes.SeparatorCharacter));
        VaultSecretPath = vaultSecretPath;
    }
}
