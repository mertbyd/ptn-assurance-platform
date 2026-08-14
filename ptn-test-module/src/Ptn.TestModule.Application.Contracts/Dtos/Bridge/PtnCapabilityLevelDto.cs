namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Hedef ortamda logical decoding, sandbox ve projeksiyon yeteneklerini tasir.
// sistemdeki gorevi: Footprint stratejisinin varsayimla degil olculen capability seviyesiyle secildigini gosterir.
public sealed class PtnCapabilityLevelDto
{
    public string FootprintStrengthCode { get; set; } = string.Empty;
    public bool HasLogicalDecoding { get; set; }
    public bool HasExclusiveSandbox { get; set; }
    public bool HasProjectionSurface { get; set; }
    public List<string> Reasons { get; set; } = [];
}
