namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: Kaynak erisilebilirlik testinin secretsiz API cevabidir.
// sistemdeki gorevi: Credential, request header'i ve Vault yolunu acmadan yalniz sonuc, dokuman sayisi ve hata durumunu bildirir.
public class SpecSourceReachabilityDto
{
    // Tum aktif dokumanlara basariyla erisilebildigini belirtir.
    public bool IsReachable { get; set; }

    // Test edilen aktif dokuman sayisi.
    public int TestedDocumentCount { get; set; }

    // Basarisiz ilk HTTP durum kodu; ag hatasinda null.
    public int? StatusCode { get; set; }

    // Basarisizligin kararli alan hata kodu; ham ag metni tasimaz, basarida null.
    public string? ErrorMessage { get; set; }
}
