namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Bir kaynagin aktif dokumanlarina yapilan erisim denemesinin secretsiz sonucunu tasir.
// sistemdeki gorevi: HTTP ayrintisini DTO'ya veya entity'ye sizdirmadan Mapperly icin guvenli uygulama cikisi saglar.
public class SpecSourceReachabilityModel
{
    // Tum aktif dokumanlara basariyla erisilebildigini belirtir.
    public bool IsReachable { get; set; }

    // Denemeye dahil edilen aktif dokuman sayisi.
    public int TestedDocumentCount { get; set; }

    // Basarisiz ilk HTTP yanitinin durum kodu; ag hatasinda null.
    public int? StatusCode { get; set; }

    // Basarisizligin kararli alan hata kodu; ham ag metni tasimaz, basarida null.
    public string? ErrorMessage { get; set; }
}
