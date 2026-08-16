namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Test edilecek yazilimin is kurali belgesinin metin icerigini yukleme girdisi olarak tasir.
// sistemdeki gorevi: Dosya adini sabit tutup yalniz icerigi kabul ederek kok disi yol denemesini sozlesme seviyesinde keser.
public sealed class UploadBusinessRulesDto
{
    public string Content { get; set; } = string.Empty;
}
