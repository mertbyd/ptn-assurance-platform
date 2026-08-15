using System;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Kalicilastirilmadan derlenecek Arazzo taslagini tanimlar.
// sistemdeki gorevi: Yazarlik ekraninin yayin kapisindan once derleme ve lint istemesini saglar.
public sealed class ScenarioCompilePreviewDto
{
    public string SourceDocument { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
}
