namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Insana yoneltilen kapali soru kodu, prompt anahtari ve seceneklerini tasir.
// sistemdeki gorevi: Esik alti adaylarin acik uclu metin veya tahmin olarak donmesini engeller.
public sealed class PtnClosedQuestionDto
{
    public string QuestionCode { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string GapKindCode { get; set; } = string.Empty;
}
