namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Envanter satirini kimlik, metot, path ve iki sema referansiyla HTTP cevabinda tasir.
// sistemdeki gorevi: Liste yolunu parametre, alan ve security yuzeyinden ayri tutar; agir ozet operation.find ucunda kalir.
public class SnapshotOperationRowDto
{
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestSchemaRef { get; set; }
    public string? ResponseSchemaRef { get; set; }
}
