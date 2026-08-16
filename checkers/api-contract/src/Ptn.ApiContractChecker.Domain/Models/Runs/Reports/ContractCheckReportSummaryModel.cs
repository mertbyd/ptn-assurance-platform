namespace Ptn.ApiContractChecker.Models.Runs.Reports;

// islevi: Bir run raporunun toplam ve severity, direction, kind kirilimlarini tasir.
// sistemdeki gorevi: Findings govdesinin tek bakista okunabilen deterministik ozetini sunar.
public class ContractCheckReportSummaryModel
{
    public int TotalFindingCount { get; set; }
    public int BreakingCount { get; set; }
    public int NonBreakingCount { get; set; }
    public int DocsOnlyCount { get; set; }
    public List<ContractCheckReportCountModel> SeverityCounts { get; set; } = [];
    public List<ContractCheckReportCountModel> DirectionCounts { get; set; } = [];
    public List<ContractCheckReportCountModel> KindCounts { get; set; } = [];
}
