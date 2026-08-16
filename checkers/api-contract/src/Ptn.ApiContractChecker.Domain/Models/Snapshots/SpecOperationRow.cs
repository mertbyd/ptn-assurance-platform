namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Envanter satirinda tek operasyonun kimligini ve iki sema adresini tasir.
// sistemdeki gorevi: Aday secimi icin gereken en az alani verir; parametre, alan ve security yuzeyi operation.find ucunda kalir.
public class SpecOperationRow
{
    // Spec bildirdiyse operasyonu tahminsiz secmeye yeten kararli kimlik.
    public string? OperationId { get; set; }

    // Operasyonun normalize edilmis buyuk harfli HTTP metodu.
    public string Method { get; set; } = string.Empty;

    // Operasyonun spec'te bildirilen path sablonu.
    public string Path { get; set; } = string.Empty;

    // Istek govdesi bir component'e bagliysa o semanin referans kimligi.
    public string? RequestSchemaRef { get; set; }

    // Ilk kararli 2xx yanit govdesi bir component'e bagliysa o semanin referans kimligi.
    public string? ResponseSchemaRef { get; set; }
}
