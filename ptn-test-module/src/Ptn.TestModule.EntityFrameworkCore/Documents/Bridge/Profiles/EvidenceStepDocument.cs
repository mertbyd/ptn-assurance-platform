using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Documents.Bridge.Profiles;

// islevi: YAML kanit adiminin kaynak, dugum ve baglama alanlarini tasir.
// sistemdeki gorevi: Adim cevirisini serbest koddan kapali domain sozlugune baglar.
internal sealed class EvidenceStepDocument
{
    public string NodeKind { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? Concept { get; set; }
    public string? JoinFrom { get; set; }
    public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.Ordinal);
}
