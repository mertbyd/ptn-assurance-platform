using Ptn.ApiContractChecker.Dtos.Correlation;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Kapali outcome kodu ve deger icermeyen ihlal listesini HTTP cikisina tasir.
// sistemdeki gorevi: Oracle kararini ve cagiranin korelasyonunu public tel sozlesmesine donusturur.
public class ConformanceResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<ConformanceViolationDto> Violations { get; set; } = new();
    public CorrelationRefDto? Correlation { get; set; }
}
