namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_explain tool'unun kapali operasyon referansi ve outcome kodu girdisini tasir.
// sistemdeki gorevi: Teshis yuzeyini serbest operasyon, tablo, kolon veya scope metninden korur.
public sealed class PtnExplainRequestDto
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
    public Guid ConnectionId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public string OutcomeCode { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string ResponseFormat { get; set; } = string.Empty;
}
