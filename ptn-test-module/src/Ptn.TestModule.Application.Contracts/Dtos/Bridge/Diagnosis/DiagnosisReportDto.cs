using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Iki checker teshisinin ortak RFC, konum ve hipotez sonucunu tasir.
// sistemdeki gorevi: Normalize teshis kontratini Application.Contracts katmanindan sunar.
public sealed class DiagnosisReportDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string SourceCheckerCode { get; set; } = string.Empty;
    /// <summary>
    /// Sozlesmenin type bilgisini belirtir.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>
    /// Sozlesmenin instance bilgisini belirtir.
    /// </summary>
    public string Instance { get; set; } = string.Empty;
    /// <summary>
    /// Bulgunun kaynak icindeki konumunu tasir.
    /// </summary>
    public LocationDto Location { get; set; } = new();
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public Dictionary<string, List<string>> Facts { get; set; } = [];
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<DiagnosisHypothesisDto> Hypotheses { get; set; } = [];
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<string> NextChecks { get; set; } = [];
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
