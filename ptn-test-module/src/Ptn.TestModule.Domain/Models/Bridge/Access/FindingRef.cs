namespace Ptn.TestModule.Models.Bridge;

// islevi: Checker bulgu parmak izini kaynak checker kimligiyle birlikte tasir.
// sistemdeki gorevi: Farkli fingerprint gramerlerinin tek ad alaninda yanlislikla birlesmesini engeller.
public sealed class FindingRef
{
    public string SourceCheckerCode { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}
