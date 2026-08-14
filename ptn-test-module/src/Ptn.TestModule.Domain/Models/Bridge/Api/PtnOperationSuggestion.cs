using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek kaynak operasyon adayi ile alan baglarini ve mekanik skorunu tasir.
// sistemdeki gorevi: Grounding manager'in esik kararini checker DTO'sundan bagimsiz yapar.
public sealed class PtnOperationSuggestion
{
    public string? SourceOperationId { get; set; }
    public string SourceMethod { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<PtnFieldBinding> Bindings { get; set; } = [];
}
