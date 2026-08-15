namespace Ptn.TestModule.Dtos.Runs;

// islevi: UTF-8 kosum artefaktinin indirilebilir metin govdesini tanimlar.
// sistemdeki gorevi: Repository evindeki Result<T> sozlesmesini stream tipi acmadan korur.
public sealed class RunArtifactContentDto
{
    public string Format { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
