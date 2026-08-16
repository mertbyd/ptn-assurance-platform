namespace Ptn.TestModule.Models.Authoring;

// islevi: Tek kapali soruya secenek kumesinden verilen cevabi tasir.
// sistemdeki gorevi: Serbest metin cevabin Manager'a sizmasini engelleyen domain girdisidir.
public sealed class AuthoringAnswerModel
{
    public string QuestionCode { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty;
}
