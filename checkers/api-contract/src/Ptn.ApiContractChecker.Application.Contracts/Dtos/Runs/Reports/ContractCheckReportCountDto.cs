namespace Ptn.ApiContractChecker.Dtos.Runs.Reports;

// islevi: Rapor kirilimindaki tek kararli lookup kodu ve sayisini API cevabinda tasir.
// sistemdeki gorevi: Severity, direction ve kind dagilimlarinin ortak cevap elemanidir.
public class ContractCheckReportCountDto
{
    public string Code { get; set; } = default!;
    public int Count { get; set; }
}
