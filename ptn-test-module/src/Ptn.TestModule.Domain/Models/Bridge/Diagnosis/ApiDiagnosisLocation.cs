namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: API checker diagnosis konumunun kaynak alan seklini tasir.
// sistemdeki gorevi: SchemaName'i ortak Location anlamina Manager'in cevirmesini saglar.
public sealed class ApiDiagnosisLocation
{
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? SchemaName { get; set; }
    public string? JsonPointer { get; set; }
}
