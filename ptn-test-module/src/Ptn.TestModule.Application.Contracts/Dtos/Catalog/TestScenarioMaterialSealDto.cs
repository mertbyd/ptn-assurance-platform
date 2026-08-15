using System;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Senaryo surumunun kurallar, API snapshot, DB semasi ve profil muhurlerini tasir.
// sistemdeki gorevi: Public katalog girdisini provider tiplerinden bagimsiz bir veri sozlesmesinde tutar.
public sealed class TestScenarioMaterialSealDto
{
    public string? RulesFingerprint { get; set; }
    public Guid? SpecSnapshotId { get; set; }
    public string? SpecFingerprint { get; set; }
    public Guid? DbConnectionId { get; set; }
    public string? DbSchemaFingerprint { get; set; }
    public string? ProfileFingerprint { get; set; }
}
