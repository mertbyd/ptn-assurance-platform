namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Profilin gerekli, bagli ve baglanmamis kavram kapsam oranini tasir.
// sistemdeki gorevi: Her zemin ve teshis cevabinda kapsami ozet alanlarinin basinda gosterir.
public sealed class PtnCoverageReportDto
{
    public List<string> RequiredConcepts { get; set; } = [];
    public List<string> BoundConcepts { get; set; } = [];
    public List<string> UnboundConcepts { get; set; } = [];
    public int BoundCount { get; set; }
    public int RequiredCount { get; set; }
    public decimal BoundRatio { get; set; }
}
