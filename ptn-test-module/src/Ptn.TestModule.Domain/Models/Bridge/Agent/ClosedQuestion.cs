using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Belirsizligi serbest cevap yerine kapali seceneklerle insana yoneltir.
// sistemdeki gorevi: Esik alti adaylarin veya eksik referanslarin tahmine donusmesini engeller.
public sealed class ClosedQuestion
{
    public string QuestionCode { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string GapKindCode { get; set; } = string.Empty;
}
