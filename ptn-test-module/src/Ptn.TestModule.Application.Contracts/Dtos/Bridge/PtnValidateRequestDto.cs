namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_validate tool'unun snapshot, operasyon ve assertion referanslarini tasir.
// sistemdeki gorevi: Serbest JSON pointer veya operasyon adresi yerine kimlik tabanli yayin kapisi girdisi verir.
public sealed class PtnValidateRequestDto
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public List<Guid> AssertionReferenceIds { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}
