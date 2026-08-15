using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Otomatik tetikleyicinin yeni kosum mu urettigini yoksa mevcut kosumu mu dondurdugunu tasir.
// sistemdeki gorevi: Idempotency kararini cagirana tek modelde bildirir; ayni tetikleyici ikinci kosum uretmez.
public class AutomatedRunOutcome
{
    public TestRun Run { get; set; } = default!;
    public bool IsNew { get; set; }
}
