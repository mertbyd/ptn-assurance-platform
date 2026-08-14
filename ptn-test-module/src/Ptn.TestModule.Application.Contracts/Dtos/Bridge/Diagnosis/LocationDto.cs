namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: API ve database bulgularinin ortak tipli konumunu tasir.
// sistemdeki gorevi: Kaynak-ozgul adresleri tek public navigation altinda toplar.
public sealed class LocationDto
{
    public string? ApiSchemaName { get; set; }
    public string? DbSchemaName { get; set; }
    public string? DbTableName { get; set; }
    public string? ColumnName { get; set; }
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? JsonPointer { get; set; }
}
