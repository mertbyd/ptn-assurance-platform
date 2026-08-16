namespace Ptn.ApiContractChecker.Dtos.Runs.Reports;

// islevi: Contract check raporunun toplam ve uc eksenli sayac ozetini API cevabinda tasir.
// sistemdeki gorevi: Findings govdesini tek bakista okunabilen lookup-kodlu dagilimlara indirger.
public class ContractCheckReportSummaryDto
{
    public int TotalFindingCount { get; set; }
    public int BreakingCount { get; set; }
    public int NonBreakingCount { get; set; }
    public int DocsOnlyCount { get; set; }
    public List<ContractCheckReportCountDto> SeverityCounts { get; set; } = [];
    public List<ContractCheckReportCountDto> DirectionCounts { get; set; } = [];
    public List<ContractCheckReportCountDto> KindCounts { get; set; } = [];
}
