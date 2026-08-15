namespace Ptn.TestModule.Models.Runs;

// islevi: Dogrulanmis UTF-8 artefakt govdesini domain read modeli olarak tasir.
// sistemdeki gorevi: Mapperly ile public DTO'ya cevrilen I/O sonucudur.
public sealed class RunArtifactContent
{
    public string Format { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
