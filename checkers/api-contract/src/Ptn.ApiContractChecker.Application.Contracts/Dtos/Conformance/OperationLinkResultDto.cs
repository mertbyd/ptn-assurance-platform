namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Kapali sonucu ve esik ustu operasyon link adaylarini public response'a tasir.
// sistemdeki gorevi: Kanit bulunmayan durumda tahmini aday eklemeden bos liste dondurur.
public class OperationLinkResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationLinkCandidateDto> Candidates { get; set; } = new();
}
