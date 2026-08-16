namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: SpecSource ve dokumanlarini tek aggregate komutuyla olusturan istek govdesidir.
// sistemdeki gorevi: Opsiyonel header kimligini yalniz giris sinirinda tasir; degerler Vault'a yazilir ve cevaplara girmez.
public class CreateSpecSourceDto
{
    // Tenant icinde benzersiz kaynak adi.
    public string Name { get; set; } = default!;

    // Dokuman yollarinin cozuldugu servis kok adresi.
    public string BaseUrl { get; set; } = default!;

    // Vault'a yazilacak HTTP kimlik basligi; HeaderValue ile birlikte veya ikisi de bos gelir.
    public string? HeaderName { get; set; }

    // Vault'a yazilacak tam HTTP baslik degeri; hicbir response veya log'a girmez.
    public string? HeaderValue { get; set; }

    // Aggregate ile birlikte tanimlanan dokumanlar; create'te Id alanlari bos olmalidir.
    public List<SpecDocumentDto> Documents { get; set; } = [];
}
