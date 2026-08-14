using System;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Senaryo surumunun kurallar, API snapshot, DB semasi ve profil malzemesi baglarini tasir.
// sistemdeki gorevi: ADR-0020 yayin kapisinin kimlik ve icerik muhurlerini tek domain modelinde degerlendirmesini saglar.
public sealed class TestScenarioMaterialSeal
{
    public string? RulesFingerprint { get; set; }
    public Guid? SpecSnapshotId { get; set; }
    public string? SpecFingerprint { get; set; }
    public Guid? DbConnectionId { get; set; }
    public string? DbSchemaFingerprint { get; set; }
    public string? ProfileFingerprint { get; set; }
}
