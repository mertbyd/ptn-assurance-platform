namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek operasyon zinciri adayini kaynak, alan eslemeleri, skor ve onay bilgisiyle tasir.
// sistemdeki gorevi: Ajanin yalniz checker tarafindan kanitlanmis hedefleri gorebilmesini saglar.
public sealed class OperationLinkCandidateDto
{
    public string TargetOperationId { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public List<OperationLinkParameterBindingDto> ParameterMap { get; set; } = [];
    public decimal Score { get; set; }
    public bool RequiresHumanApproval { get; set; }
}
