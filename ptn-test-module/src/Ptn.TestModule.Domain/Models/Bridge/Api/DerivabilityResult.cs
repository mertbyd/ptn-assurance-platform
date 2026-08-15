using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Her assertion JSON pointer'i icin normalize edilmis turetilebilirlik sonucunu tasir.
// sistemdeki gorevi: RULE-0006 yayin kapisini checker DTO'su ve mesaj metninden bagimsiz besler.
public sealed class DerivabilityResult
{
    public List<DerivabilityItem> Assertions { get; set; } = [];
    public bool IsTruncated { get; set; }
}
