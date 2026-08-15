using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kavram baglamalari, kanit yollari ve sema muhru bulunan profil paketini tasir.
// sistemdeki gorevi: Ortama ozgu alan bilgisini tablo yerine Git'te surumlenen dosyada tutar.
public sealed class ProfilePack
{
    public string ProfileKey { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string DbSchemaFingerprint { get; set; } = string.Empty;
    public Guid? SpecSnapshotId { get; set; }
    public List<ConceptBinding> Bindings { get; set; } = [];
    public List<EvidencePathDefinition> Paths { get; set; } = [];
    public string ContentFingerprint { get; set; } = string.Empty;
}
