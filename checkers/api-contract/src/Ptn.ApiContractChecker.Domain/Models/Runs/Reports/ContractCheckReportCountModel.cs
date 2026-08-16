namespace Ptn.ApiContractChecker.Models.Runs.Reports;

// islevi: Rapor ozetindeki tek kararli severity, direction veya kind kodunun sayisini tasir.
// sistemdeki gorevi: Rapor tuketicisinin lookup kodlarini kendi gorunen adlariyla eslestirebilmesini saglar.
public class ContractCheckReportCountModel
{
    public string Code { get; set; } = default!;
    public int Count { get; set; }
}
