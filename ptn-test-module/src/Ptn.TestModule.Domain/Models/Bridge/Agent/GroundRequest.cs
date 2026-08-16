using System;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Tek ptn_ground cagrisinin profil, snapshot, baglanti ve kapali operasyon referansini tasir.
// sistemdeki gorevi: StepIntent disinda serbest operasyon, tablo, kolon veya scope metni tasinmasini engeller.
public sealed class GroundRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
    public Guid ConnectionId { get; set; }
    public Guid? OperationReferenceId { get; set; }
    public Guid? TableReferenceId { get; set; }
    public string StepIntent { get; set; } = string.Empty;
    public string ResponseFormat { get; set; } = string.Empty;
    public bool HasExclusiveSandbox { get; set; }
}
