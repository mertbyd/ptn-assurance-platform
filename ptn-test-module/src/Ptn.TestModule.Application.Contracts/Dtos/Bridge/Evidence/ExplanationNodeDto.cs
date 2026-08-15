using Ptn.TestModule.Dtos.Bridge.Diagnosis;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Yurutulen kanit adiminin durum, alaka, konum, kanit ve alt dugumlerini tasir.
// sistemdeki gorevi: Aciklamayi ikinci akil yurutme yerine mekanik yurutme izi olarak sunar.
public sealed class ExplanationNodeDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string NodeKindCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string StateCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string RelevanceCode { get; set; } = string.Empty;
    /// <summary>
    /// Bulgunun kaynak icindeki konumunu tasir.
    /// </summary>
    public LocationDto Location { get; set; } = new();
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<EvidenceDto> Evidence { get; set; } = [];
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ExplanationNodeDto> Children { get; set; } = [];
}
