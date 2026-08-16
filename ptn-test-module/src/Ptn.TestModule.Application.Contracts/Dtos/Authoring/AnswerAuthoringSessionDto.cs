namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Oturumdaki tek kapali soruya secenek kumesinden verilen cevabi tasir.
// sistemdeki gorevi: Serbest metin cevabi authoring endpoint sozlesmesinin disinda tutar.
public sealed class AnswerAuthoringSessionDto
{
    public string QuestionCode { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty;
}
