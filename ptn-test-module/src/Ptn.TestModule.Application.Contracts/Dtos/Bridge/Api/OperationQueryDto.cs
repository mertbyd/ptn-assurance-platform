namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: API operasyonu secmek icin snapshot ve HTTP adresini tasir.
// sistemdeki gorevi: Bridge AppService girdisini Domain modelinden ayri tutar.
public sealed class OperationQueryDto
{
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public Guid SnapshotId { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? OperationId { get; set; }
    /// <summary>
    /// HTTP operasyonunun yontemini belirtir.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
