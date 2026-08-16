namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: Checker operasyon envanterinin referans uretilmeden onceki hafif satirini tasir.
// sistemdeki gorevi: External wire alanlarini Mapperly ile kayipsiz alip referans kararini Manager'a birakir.
public sealed class SnapshotOperationRow
{
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestSchemaRef { get; set; }
    public string? ResponseSchemaRef { get; set; }
}
