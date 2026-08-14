namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Hedef ortamda logical decoding, sandbox ve projeksiyon yeteneklerini tasir.
// sistemdeki gorevi: Footprint stratejisinin varsayimla degil olculen capability seviyesiyle secildigini gosterir.
public sealed class CapabilityLevelDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string FootprintStrengthCode { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool HasLogicalDecoding { get; set; }
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool HasExclusiveSandbox { get; set; }
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool HasProjectionSurface { get; set; }
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<string> Reasons { get; set; } = [];
}
