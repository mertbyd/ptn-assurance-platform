using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Idempotent kosum claim kararini guncel aggregate ile birlikte tasir.
// sistemdeki gorevi: Application katmaninin Mapperly ile public claim DTO'su uretmesini saglar.
/// <summary>Bir TestRun claim denemesinin domain sonucudur.</summary>
public class TestRunClaimResult
{
    /// <summary>Bu denemenin Pending kosumu Running durumuna gecirip gecirmedigidir.</summary>
    public bool Claimed { get; set; }

    /// <summary>Claim denemesi sonrasindaki guncel TestRun aggregate'idir.</summary>
    public TestRun Run { get; set; } = null!;
}
