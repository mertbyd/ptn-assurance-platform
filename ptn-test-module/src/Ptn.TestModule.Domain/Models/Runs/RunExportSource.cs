using System.Collections.Generic;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir kosumu ve onun tum terminal denemelerini ihracat hesabinin tek girdisinde toplar.
// sistemdeki gorevi: CTRF, JUnit ve SARIF uretiminin model cagrisi olmadan saf hesapla kosmasini saglar (RULE-0005, PLAN-0003 TM-14/TM-30).
/// <summary>
/// Kosum ihracatinin deterministik girdisini tasir.
/// </summary>
public class RunExportSource
{
    /// <summary>Ihracatin ait oldugu TestRun aggregate'idir.</summary>
    public TestRun Run { get; set; } = null!;

    /// <summary>Kosumun deneme numarasina gore artan sirali terminal denemeleridir.</summary>
    public IReadOnlyList<RunExportAttempt> Attempts { get; set; } = [];
}
