using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir kosumu ve onun en son terminal denemesini bulgulariyla birlikte tek okuma modelinde tasir.
// sistemdeki gorevi: Application katmaninin Mapperly ile tek public teshis raporu DTO'su uretmesini saglar (PLAN-0003 TM-22).
/// <summary>Bir test kosumunun terminal teshis raporunun domain okuma modelidir.</summary>
public class TestRunReport
{
    /// <summary>Raporun ait oldugu TestRun aggregate'idir.</summary>
    public TestRun Run { get; set; } = null!;

    /// <summary>Kosumun en son terminal denemesidir; henuz terminale girmemisse null kalir.</summary>
    public TestRunResult? Result { get; set; }
}
