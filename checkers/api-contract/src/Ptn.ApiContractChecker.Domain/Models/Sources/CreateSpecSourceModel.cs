namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Yeni spec kaynaginin kalici ve aggregate-cocuk alanlarini tasir.
// sistemdeki gorevi: Secret degerlerini disarida birakir; manager yalniz Vault yolunu ve kaynak tanimini gorur.
public class CreateSpecSourceModel
{
    // Tenant icinde benzersiz kaynak adi.
    public string Name { get; set; } = default!;

    // Dokuman yollarinin cozuldugu servis kok adresi.
    public string BaseUrl { get; set; } = default!;

    // AppService'in Vault yazimindan sonra ekledigi secret adresi.
    public string? VaultSecretPath { get; set; }

    // Aggregate ile birlikte kurulacak dokuman tanimlari.
    public List<SpecDocumentModel> Documents { get; set; } = [];
}
