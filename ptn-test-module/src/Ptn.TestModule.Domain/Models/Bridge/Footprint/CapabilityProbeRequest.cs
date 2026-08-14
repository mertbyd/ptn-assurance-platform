using System;

namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Capability yoklamasi icin baglanti ve sandbox beklentisini birlikte tasir.
// sistemdeki gorevi: Application servisinin checker request DTO'su kurmasini engelleyen domain siniridir.
public sealed class CapabilityProbeRequest
{
    public Guid ConnectionId { get; set; }
    public bool RequiresExclusiveSandbox { get; set; }
}
