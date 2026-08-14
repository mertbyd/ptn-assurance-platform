using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API veya database teshisi icin ortak referans ve yapilandirilmis sinyal alanlarini tasir.
// sistemdeki gorevi: Iki checker teshis DTO'sunu tek kopru port girdisinde birlestirir.
public sealed class PtnDiagnosisRequest
{
    public Guid? SpecSnapshotId { get; set; }
    public Guid? ApiRunId { get; set; }
    public Guid ConnectionId { get; set; }
    public PtnLocation Location { get; set; } = new();
    public int? StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? OutcomeCode { get; set; }
    public string? TransportErrorCode { get; set; }
    public string? EngineCode { get; set; }
    public string? SqlState { get; set; }
    public long? ObservedAtMs { get; set; }
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<PtnAssertionResult.PtnFailedExpectation> FailedExpectations { get; set; } = [];
    public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
