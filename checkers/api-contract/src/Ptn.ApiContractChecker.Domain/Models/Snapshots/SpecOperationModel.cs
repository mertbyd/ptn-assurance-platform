namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Tek bir path ve HTTP metodu altindaki operasyon yuzeyini tasir.
// sistemdeki gorevi: Endpoint kimligi ile parametre, govde ve yanit sozlesmesini ayni domain nesnesinde toplar.
public class SpecOperationModel
{
    // Spec'te bildirilen path sablonu.
    public string Path { get; set; } = string.Empty;

    // Operasyonun HTTP metodu.
    public string Method { get; set; } = string.Empty;

    // Varsa spec tarafindan bildirilen kararli operasyon kimligi.
    public string? OperationId { get; set; }

    // Operasyonun x-internal true ile dis kullanima kapali isaretlenip isaretlenmedigi.
    public bool IsInternal { get; set; }

    // Operasyonu gruplandiran tag adlari.
    public List<string> Tags { get; set; } = new();

    // Operasyonda etkili olan guvenlik gereksinimleri.
    public List<SpecSecurityRequirementModel> SecurityRequirements { get; set; } = new();

    // Path ve operasyon seviyesinden birlestirilen parametreler.
    public List<SpecParameterModel> Parameters { get; set; } = new();

    // Her medya tipi icin istek govdesi sozlesmesi.
    public List<SpecRequestBodyModel> RequestBodies { get; set; } = new();

    // Durum kodu ve medya tipi bazindaki yanit sozlesmeleri.
    public List<SpecResponseModel> Responses { get; set; } = new();
}
