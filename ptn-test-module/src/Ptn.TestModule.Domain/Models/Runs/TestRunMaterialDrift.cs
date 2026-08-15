using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum aninda kayan senaryo malzemelerini kararli kodlarla tasir.
// sistemdeki gorevi: Kaymayi Failed yerine Inconclusive terminal modeline cevirecek kaniti tasir.
/// <summary>
/// Senaryo malzeme muhurlerinin kosum ani karsilastirma sonucunu tasir.
/// </summary>
public class TestRunMaterialDrift
{
    /// <summary>En az bir malzeme muhrunun kayip kaymadigini belirtir.</summary>
    public bool HasDrift { get; set; }

    /// <summary>Kaymis malzemelerin kararli ve raporlanabilir kodlaridir.</summary>
    public IReadOnlyCollection<string> DriftedMaterialCodes { get; set; } = [];
}
