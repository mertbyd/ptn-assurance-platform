using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit yolunun gerektirdigi ve profilce baglanan kavramlarin kapsam sonucunu tasir.
// sistemdeki gorevi: Kapsam disi soruyu sessiz tahmin yerine olculebilir NOT_BOUND sonucu yapar.
public sealed class PtnCoverageReport
{
    public List<string> RequiredConcepts { get; set; } = [];
    public List<string> BoundConcepts { get; set; } = [];
    public List<string> UnboundConcepts { get; set; } = [];
    public int BoundCount { get; set; }
    public int RequiredCount { get; set; }
    public decimal BoundRatio { get; set; }
}
