using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Tipli bir veritabani adimi onerisi ile kapali operasyon referansini tasir.
// sistemdeki gorevi: LLM'in serbest metin kullanmadan DB assertion'i uretebilmesini saglar.
public sealed class AddDatabaseAuthoringStepDto
{
    public string StepId { get; set; } = string.Empty;
    public Guid TableReferenceId { get; set; }
    public string OperationCode { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyBindings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AuthoringExpectationDto> Expectations { get; set; } = [];
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
}
