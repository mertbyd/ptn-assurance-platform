namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Raporun snapshot operation, schema, property ve JSON Pointer konumunu tasir.
// sistemdeki gorevi: Lokalize anlatimi makine-okunur katalog adresiyle birlikte dondurur.
public sealed class ObjectReferenceDto
{
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? SchemaName { get; set; }
    public string? PropertyPath { get; set; }
    public string? JsonPointer { get; set; }
}
