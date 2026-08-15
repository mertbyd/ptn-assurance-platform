using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kaynak operasyon icin kanitli zincir adaylarini kapali sonucuyla tasir.
// sistemdeki gorevi: Aday bulunmamasini tahminle doldurmadan Bridge yazarlik yuzeyine iletir.
public sealed class OperationLinkResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationLinkCandidate> Candidates { get; set; } = [];
}
