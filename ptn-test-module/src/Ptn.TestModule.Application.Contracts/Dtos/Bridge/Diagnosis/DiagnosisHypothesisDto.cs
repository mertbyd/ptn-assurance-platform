namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Tek teshis hipotezinin oncelik, guven, kanit ve sonraki kontrollerini tasir.
// sistemdeki gorevi: Kaynakli hipotezi public diagnosis kontratinda sunar.
public sealed class DiagnosisHypothesisDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string HypothesisKindCode { get; set; } = string.Empty;
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int Priority { get; set; }
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string ConfidenceCode { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public FindingRefDto Ref { get; set; } = new();
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<EvidenceDto> Evidence { get; set; } = [];
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<string> NextChecks { get; set; } = [];
}
