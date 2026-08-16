namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: UOW disindaki comparison'in run kimligiyle birlikte terminal bulgularini tasir.
// sistemdeki gorevi: Saf motor sonucunu CompleteAsync kisa UOW'sine kayipsiz aktarir.
public class ContractCheckExecutionResultModel
{
    // Terminal sonucu yazilacak run kimligi.
    public Guid RunId { get; set; }

    // Normalize edilmis iki snapshot arasindaki siniflandirilmis bulgular.
    public ContractCheckFindings Findings { get; set; } = ContractCheckFindings.Empty();
}
