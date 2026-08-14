namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Response assertion yollarinin turetilebilirlik istegini tasir.
// sistemdeki gorevi: Public girdiyi tipli operasyon ve assertion alanlariyla sinirlar.
public sealed class DerivabilityRequestDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? StatusCode { get; set; }
    public string? MediaType { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
