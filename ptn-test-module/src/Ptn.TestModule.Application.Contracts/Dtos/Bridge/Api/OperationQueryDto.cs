namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: API operasyonu secmek icin snapshot ve HTTP adresini tasir.
// sistemdeki gorevi: Bridge AppService girdisini Domain modelinden ayri tutar.
public sealed class OperationQueryDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
