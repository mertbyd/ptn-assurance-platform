namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir durum kodu ve medya tipi icin yanit sozlesmesini tasir.
// sistemdeki gorevi: Yanit kimligini, sema adresini ve header yuzeyini tek karsilastirma biriminde toplar.
public class SpecResponseModel
{
    // HTTP durum kodu veya spec'teki default yanit anahtari.
    public string StatusCode { get; set; } = string.Empty;

    // Yanit govdesinin medya tipi; govde yoksa bostur.
    public string MediaType { get; set; } = string.Empty;

    // Yanit semasi bir component'e bagliysa korunacak referans kimligi.
    public string? SchemaReferenceId { get; set; }

    // Yanit govdesinin inline veya cozulmus validation semasi.
    public SpecSchemaModel? Schema { get; set; }

    // Yanitla birlikte donen header sozlesmeleri.
    public List<SpecHeaderModel> Headers { get; set; } = new();

    // Yanit degerlerini izleyen operasyon girdilerine baglayan OpenAPI link beyanlari.
    public List<SpecOperationLinkModel> Links { get; set; } = new();
}
