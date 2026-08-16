namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Profil paketinin anahtarini ve YAML metin icerigini yukleme girdisi olarak tasir.
// sistemdeki gorevi: Dosya adini anahtardan turetip serbest yol kabulunu sozlesme seviyesinde engeller.
public sealed class UploadProfilePackDto
{
    public string ProfileKey { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
