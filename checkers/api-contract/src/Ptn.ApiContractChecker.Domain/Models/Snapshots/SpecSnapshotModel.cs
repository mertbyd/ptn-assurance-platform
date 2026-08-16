namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir API spec'inin karsilastirilabilir operasyon ve sema fotografini tasir.
// sistemdeki gorevi: Her format okuyucusu bu saf modeli doldurur; normalizasyon ve sonraki diff adimlari provider tiplerini gormez.
public class SpecSnapshotModel
{
    // Runtime operation cozumunde kullanilan info.version; mevcut CanonicalHash girdisine sonradan eklenmez.
    public string? ApiVersion { get; set; }

    // Runtime path cozumunde kullanilan server URL'leri; mevcut CanonicalHash girdisine sonradan eklenmez.
    public List<string> Servers { get; set; } = new();

    // Spec'teki endpoint yuzeyi.
    public List<SpecOperationModel> Operations { get; set; } = new();

    // Spec'teki yeniden kullanilabilir semalar.
    public List<SpecSchemaModel> Schemas { get; set; } = new();

    // Yapisal modelden ayri tutulan ve yalniz dokumantasyon olarak siniflandirilacak metinler.
    public List<SpecDocumentationModel> Documentation { get; set; } = new();
}
