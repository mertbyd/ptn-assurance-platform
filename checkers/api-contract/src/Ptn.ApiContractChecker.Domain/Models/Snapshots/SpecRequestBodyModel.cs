namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir operasyonun tek medya tipindeki istek govdesi sozlesmesini tasir.
// sistemdeki gorevi: Govde zorunlulugu ve sema adresini medya tipi kimligiyle karsilastirilabilir hale getirir.
public class SpecRequestBodyModel
{
    // Govdenin istemci tarafindan gonderilmesinin zorunlu olup olmadigi.
    public bool Required { get; set; }

    // Govdenin medya tipi.
    public string MediaType { get; set; } = string.Empty;

    // Govde semasi bir component'e bagliysa korunacak referans kimligi.
    public string? SchemaReferenceId { get; set; }

    // Runtime request validation ve minimal ornek icin inline veya cozulmus sema.
    public SpecSchemaModel? Schema { get; set; }
}
