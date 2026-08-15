namespace Ptn.TestModule.Models.Runs;

// islevi: Okunacak UTF-8 artefaktin format, blob ve content-type kararini tasir.
// sistemdeki gorevi: AppService I/O'sunu format kararindan ayirir.
public sealed class RunArtifactDescriptor
{
    public string Format { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
