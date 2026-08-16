namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: SpecSource ve aggregate dokumanlarinin guncelleme niyetini tasir.
// sistemdeki gorevi: Mevcut secret yolunu koruma veya yeni Vault yoluna gecme kararini secret degeri tasimadan domain'e iletir.
public class UpdateSpecSourceModel
{
    // Tenant icinde benzersiz kaynak adi.
    public string Name { get; set; } = default!;

    // Dokuman yollarinin cozuldugu servis kok adresi.
    public string BaseUrl { get; set; } = default!;

    // Mevcut veya yeni yazilmis kimlik bilgisinin Vault adresi.
    public string? VaultSecretPath { get; set; }

    // Eklenecek, guncellenecek veya pasiflestirilecek dokuman tanimlari.
    public List<SpecDocumentModel> Documents { get; set; } = [];
}
