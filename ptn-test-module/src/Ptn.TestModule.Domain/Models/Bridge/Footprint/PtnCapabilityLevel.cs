using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Hedef veritabaninin yazma kumesi gozlem yeteneklerini tasir.
// sistemdeki gorevi: Strateji secimini motor, logical decoding, sandbox ve projeksiyon olgularina baglar.
public sealed class PtnCapabilityLevel
{
    public string FootprintStrengthCode { get; set; } = string.Empty;
    public bool HasLogicalDecoding { get; set; }
    public bool HasExclusiveSandbox { get; set; }
    public bool HasProjectionSurface { get; set; }
    public List<string> Reasons { get; set; } = [];
}
