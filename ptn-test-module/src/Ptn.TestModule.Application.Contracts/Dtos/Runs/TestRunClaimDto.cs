namespace Ptn.TestModule.Dtos.Runs;

// islevi: Idempotent Pending -> Running claim sonucunu tasir.
// sistemdeki gorevi: Yeniden teslimi exception yerine acik bool ve guncel kosum gorunumuyle bildirir.
/// <summary>Bir test kosumu claim denemesinin public sonucudur.</summary>
public sealed class TestRunClaimDto
{
    /// <summary>Bu istegin Pending kosumu Running durumuna gecirip gecirmedigidir.</summary>
    public bool Claimed { get; set; }

    /// <summary>Claim denemesi sonrasindaki guncel kosum gorunumudur.</summary>
    public TestRunDto Run { get; set; } = new();
}
