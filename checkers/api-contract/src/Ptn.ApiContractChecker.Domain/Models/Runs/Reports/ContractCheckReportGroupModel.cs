namespace Ptn.ApiContractChecker.Models.Runs.Reports;

// islevi: Ayni difference kind kodundaki bulgulari sayisi ve deterministik sirali satirlariyla gruplar.
// sistemdeki gorevi: Rapor detayini finding satirlarini ikinci bir kalici modele donusturmeden bolumlere ayirir.
public class ContractCheckReportGroupModel
{
    public string KindCode { get; set; } = default!;
    public int FindingCount { get; set; }
    public List<Finding> Findings { get; set; } = [];
}
