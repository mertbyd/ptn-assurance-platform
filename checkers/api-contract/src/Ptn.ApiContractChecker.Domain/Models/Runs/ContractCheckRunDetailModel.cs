namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Contract check run basligini tam owned bulgu govdesiyle birlikte tasir.
// sistemdeki gorevi: Agir JSON kolonunu yalniz detay ve rapor kullanim senaryolarina acar.
public class ContractCheckRunDetailModel : ContractCheckRunHeaderModel
{
    public ContractCheckFindings Findings { get; set; } = ContractCheckFindings.Empty();
}
