using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit yolundaki tek deterministik probe adiminin veri tanimini tasir.
// sistemdeki gorevi: Dugum kaynagi, kavrami ve onceki dugum bagini sirali yurutmeye verir.
public sealed class PtnEvidencePathStep
{
    [YamlMember(Alias = "nodeKind")]
    public string NodeKindCode { get; set; } = string.Empty;
    [YamlMember(Alias = "source")]
    public string SourceCode { get; set; } = string.Empty;
    [YamlMember(Alias = "concept")]
    public string? ConceptCode { get; set; }
    [YamlMember(Alias = "joinFrom")]
    public string? JoinFromNodeKindCode { get; set; }
    public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.Ordinal);
}
