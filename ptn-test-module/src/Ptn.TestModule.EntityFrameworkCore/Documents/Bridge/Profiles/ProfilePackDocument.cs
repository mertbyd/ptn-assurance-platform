using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Documents.Bridge.Profiles;

// islevi: YAML profil belgesinin kok transport seklini tasir.
// sistemdeki gorevi: Dis dosya semasini domain modelinden ayri ve dar tutar.
internal sealed class ProfilePackDocument
{
    public string ProfileKey { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string DbSchemaFingerprint { get; set; } = string.Empty;
    public Guid? SpecSnapshotId { get; set; }
    public List<ConceptBindingDocument> Bindings { get; set; } = [];
    public List<EvidencePathDocument> Paths { get; set; } = [];
}
