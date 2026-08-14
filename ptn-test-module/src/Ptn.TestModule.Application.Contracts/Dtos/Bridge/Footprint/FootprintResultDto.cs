namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Yazma kumesi gucu, nesne ozeti ve advisory bayragini public kontratta tasir.
// sistemdeki gorevi: Exact dahil footprint'in onaysiz assertion oracle'i olmadigini istemciye bildirir.
public sealed class FootprintResultDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string StrengthCode { get; set; } = string.Empty;
    /// <summary>
    /// Isleme katilan tablo adreslerini listeler.
    /// </summary>
    public List<string> Tables { get; set; } = [];
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<string> Columns { get; set; } = [];
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<RowDeltaDto> RowDeltas { get; set; } = [];
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool IsAdvisoryOnly { get; set; } = true;
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<string> Reasons { get; set; } = [];
}
