using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek hedef operasyon adayini kanit kaynagi, parametre eslemeleri ve skoruyla tasir.
// sistemdeki gorevi: Mekanik checker onerisi ile zorunlu insan onayini birlikte korur.
public sealed class OperationLinkCandidate
{
    public string TargetOperationId { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public List<OperationLinkParameterBinding> ParameterMap { get; set; } = [];
    public decimal Score { get; set; }
    public bool RequiresHumanApproval { get; set; }
}
