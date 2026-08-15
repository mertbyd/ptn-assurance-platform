namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Insana yoneltilen kapali soru kodu, prompt anahtari ve seceneklerini tasir.
// sistemdeki gorevi: Esik alti adaylarin acik uclu metin veya tahmin olarak donmesini engeller.
public sealed class ClosedQuestionDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string QuestionCode { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<string> Options { get; set; } = [];
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string GapKindCode { get; set; } = string.Empty;
}
