using System;

namespace Ptn.TestModule.Models.Compilation;

// islevi: Kalici olsun veya olmasin bir senaryonun yayin kapisina girecek kaynak ve malzeme muhrunu tasir.
// sistemdeki gorevi: MCP validate ile katalog yayininin ayni derleme ve gate sahiplerini kullanmasini saglar.
public sealed class ScenarioPublicationCandidate
{
    public string SourceDocument { get; set; } = string.Empty;
    public string? RulesFingerprint { get; set; }
    public Guid? SpecSnapshotId { get; set; }
    public string? SpecFingerprint { get; set; }
    public Guid? DbConnectionId { get; set; }
    public string? DbSchemaFingerprint { get; set; }
    public string? ProfileFingerprint { get; set; }
}
