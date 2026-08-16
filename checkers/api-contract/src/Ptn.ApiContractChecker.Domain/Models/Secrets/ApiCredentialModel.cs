namespace Ptn.ApiContractChecker.Models.Secrets;

// islevi: Vault'tan cozulmus spec kaynagi kimlik bilgisini tasiyan domain modeli.
// sistemdeki gorevi: Spec cekilirken istege eklenecek kimlik dogrulama basligini tasir; yalniz bellekte yasar, DB'ye yazilmaz, DTO'ya ve log'a girmez.
public class ApiCredentialModel
{
    // Istege eklenecek basligin adi ("Authorization", "X-Api-Key" ...); sema kaynak servise gore degisir.
    public string HeaderName { get; set; } = default!;

    // Basligin tam degeri ("Bearer eyJ..." veya ham anahtar); asla loglanmaz, asla DTO'ya konmaz.
    public string HeaderValue { get; set; } = default!;
}
