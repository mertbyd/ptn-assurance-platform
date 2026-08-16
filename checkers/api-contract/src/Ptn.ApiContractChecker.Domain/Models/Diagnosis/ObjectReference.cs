namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Hata veya kanitin snapshot icindeki operasyon, sema ve alan adresini tasir.
// sistemdeki gorevi: Hipotezleri serbest metin yerine dogrulanmis katalog konumuna baglar.
public sealed class ObjectReference
{
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? SchemaName { get; set; }
    public string? PropertyPath { get; set; }
    public string? JsonPointer { get; set; }
}
