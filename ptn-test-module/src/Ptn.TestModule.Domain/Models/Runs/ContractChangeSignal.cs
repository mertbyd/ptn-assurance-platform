using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: API Checker olayindan okunan bulgu agirligini modul ici karar girdisine cevirir.
// sistemdeki gorevi: Handler'in checker ETO tipini Manager'a sizdirmadan karar sormasini saglar (ADR-0015 §F).
public class ContractChangeSignal
{
    public Guid CheckRunId { get; set; }
    public Guid? TenantId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public int NewFindingCount { get; set; }
    public string? MaxSeverityCode { get; set; }
}
