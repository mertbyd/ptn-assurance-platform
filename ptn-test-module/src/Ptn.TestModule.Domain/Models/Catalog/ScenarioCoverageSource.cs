using System;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Kapsam hesabinin okudugu tek yayinlanmis senaryonun derleme girdisini tasir.
// sistemdeki gorevi: Derlenmis belge yalniz Manager'in ayristirma girdisidir; hicbir yanitta yer almaz (TM-22).
public class ScenarioCoverageSource
{
    public Guid ScenarioId { get; set; }
    public string ScenarioKey { get; set; } = string.Empty;
    public Guid? SpecSnapshotId { get; set; }
    public string CompiledDocument { get; set; } = string.Empty;
}
