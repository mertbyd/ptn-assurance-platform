namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_ground tool'unun profil, snapshot, baglanti ve kapali referans girdilerini tasir.
// sistemdeki gorevi: StepIntent disinda serbest operasyon, tablo, kolon, kod veya scope metni tasimaz.
public sealed class PtnGroundRequestDto
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
    public Guid ConnectionId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public Guid? TableReferenceId { get; set; }
    public string StepIntent { get; set; } = string.Empty;
    public string ResponseFormat { get; set; } = string.Empty;
    public bool HasExclusiveSandbox { get; set; }
}
