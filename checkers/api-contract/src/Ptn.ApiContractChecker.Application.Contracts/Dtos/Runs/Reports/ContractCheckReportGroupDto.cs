using Ptn.ApiContractChecker.Dtos.Runs;

namespace Ptn.ApiContractChecker.Dtos.Runs.Reports;

// islevi: Ayni difference kind kodundaki deterministik sirali bulgu listesini API cevabinda tasir.
// sistemdeki gorevi: Rapor detayini lookup turune gore bolumleyerek istemcinin ek islem yapmadan gostermesini saglar.
public class ContractCheckReportGroupDto
{
    public string KindCode { get; set; } = default!;
    public int FindingCount { get; set; }
    public List<FindingDto> Findings { get; set; } = [];
}
