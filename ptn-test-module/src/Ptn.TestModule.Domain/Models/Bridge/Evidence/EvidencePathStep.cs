using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit yolundaki tek deterministik probe adiminin veri tanimini tasir.
// sistemdeki gorevi: Dugum kaynagi, kavrami ve onceki dugum bagini sirali yurutmeye verir.
public sealed class EvidencePathStep
{
    public string NodeKindCode { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public string? ConceptCode { get; set; }
    public string? JoinFromNodeKindCode { get; set; }
    public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.Ordinal);
}
