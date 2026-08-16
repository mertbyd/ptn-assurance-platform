namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Tek operasyon link adayini kaynak, parametre eslemeleri, skor ve onay bayragiyla tasir.
// sistemdeki gorevi: Checker onerisi ile insan kararini public sozlesmede kesin olarak ayirir.
public class OperationLinkCandidateDto
{
    public string TargetOperationId { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public List<OperationLinkParameterBindingDto> ParameterMap { get; set; } = new();
    public decimal Score { get; set; }
    public bool RequiresHumanApproval { get; set; }
}
