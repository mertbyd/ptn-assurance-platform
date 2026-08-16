namespace Ptn.DatabaseChecker.Dtos.Capabilities;

// islevi: Yazma kumesi capability yoklamasinin kayitli baglanti ve sandbox tekillik girdisini tasir.
// sistemdeki gorevi: Probe HTTP govdesini Application.Contracts katmaninda kararli public sekle baglar.
public sealed class CapabilityProbeRequestDto
{
    public Guid ConnectionId { get; set; }
    public bool RequiresExclusiveSandbox { get; set; }
}
