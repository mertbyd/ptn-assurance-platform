using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Her assertion JSON pointer'i icin normalize edilmis turetilebilirlik sonucunu tasir.
// sistemdeki gorevi: RULE-0006 yayin kapisini checker DTO'su ve mesaj metninden bagimsiz besler.
public sealed class PtnDerivabilityResult
{
    public List<PtnDerivabilityItem> Assertions { get; set; } = [];
    public bool IsTruncated { get; set; }

    // islevi: Tek assertion pointer'i ve kapali outcome kodunu tasir.
    // sistemdeki gorevi: Turetilemeyen assertion'i dogrudan kaynak konumuyla raporlar.
    public sealed class PtnDerivabilityItem
    {
        public string JsonPointer { get; set; } = string.Empty;
        public string OutcomeCode { get; set; } = string.Empty;
    }
}
