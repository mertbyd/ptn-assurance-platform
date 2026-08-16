namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Snapshot envanterindeki tek gercek operasyonu kapali referansiyla istemciye tasir.
// sistemdeki gorevi: OperationId, method ve path bilgisini checker transport tipinden ayirir.
public sealed class SnapshotOperationDto
{
    public Guid ReferenceId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestSchemaRef { get; set; }
    public string? ResponseSchemaRef { get; set; }
}
