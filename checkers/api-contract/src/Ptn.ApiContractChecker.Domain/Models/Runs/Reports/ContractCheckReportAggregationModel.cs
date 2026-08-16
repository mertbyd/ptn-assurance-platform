namespace Ptn.ApiContractChecker.Models.Runs.Reports;

// islevi: Findings'ten her istekte uretilen raporun ozet ve kind bazli gruplarini birlestirir.
// sistemdeki gorevi: Kalici bir report kolonu olmadan API rapor cevabinin domain kaynagini olusturur.
public class ContractCheckReportAggregationModel
{
    public ContractCheckReportSummaryModel Summary { get; set; } = new();
    public List<ContractCheckReportGroupModel> Groups { get; set; } = [];
}
