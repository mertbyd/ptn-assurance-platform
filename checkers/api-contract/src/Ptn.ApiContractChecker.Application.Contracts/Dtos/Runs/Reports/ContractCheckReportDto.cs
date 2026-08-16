namespace Ptn.ApiContractChecker.Dtos.Runs.Reports;

// islevi: Findings'ten anlik uretilen ozet ve gruplu contract check raporunu API cevabinda birlestirir.
// sistemdeki gorevi: Kalici report kolonu olmadan deterministik rapor endpointi sozlesmesini sunar.
public class ContractCheckReportDto
{
    public ContractCheckReportSummaryDto Summary { get; set; } = new();
    public List<ContractCheckReportGroupDto> Groups { get; set; } = [];
}
