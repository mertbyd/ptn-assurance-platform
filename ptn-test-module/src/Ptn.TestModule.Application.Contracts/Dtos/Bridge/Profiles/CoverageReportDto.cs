namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Profilin gerekli, bagli ve baglanmamis kavram kapsam oranini tasir.
// sistemdeki gorevi: Her zemin ve teshis cevabinda kapsami ozet alanlarinin basinda gosterir.
public sealed class CoverageReportDto
{
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> RequiredConcepts { get; set; } = [];
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> BoundConcepts { get; set; } = [];
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> UnboundConcepts { get; set; } = [];
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int BoundCount { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int RequiredCount { get; set; }
    /// <summary>
    /// Karar veya eslesme icin kullanilan sayisal olcuyu belirtir.
    /// </summary>
    public decimal BoundRatio { get; set; }
}
