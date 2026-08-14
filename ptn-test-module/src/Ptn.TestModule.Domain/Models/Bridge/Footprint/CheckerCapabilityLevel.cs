using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Database checker capability sonucunu kaynak property adlariyla tasir.
// sistemdeki gorevi: Mapperly eslemesini semantik Bridge kararindan ayiran gecici domain kabugudur.
public sealed class CheckerCapabilityLevel
{
    public string StrengthCode { get; set; } = string.Empty;
    public bool HasLogicalDecoding { get; set; }
    public bool HasExclusiveSandbox { get; set; }
    public List<string> Reasons { get; set; } = [];
}
