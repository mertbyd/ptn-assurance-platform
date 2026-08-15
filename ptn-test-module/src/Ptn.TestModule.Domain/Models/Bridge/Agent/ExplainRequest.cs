using System;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Tek ptn_explain cagrisinin kapali referanslarini ve gozlenen outcome kodunu tasir.
// sistemdeki gorevi: Teshis yuzeyine serbest operasyon, tablo, kolon veya scope metni girmesini engeller.
public sealed class ExplainRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
    public Guid ConnectionId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public string OutcomeCode { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string ResponseFormat { get; set; } = string.Empty;
}
