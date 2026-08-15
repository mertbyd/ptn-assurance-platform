using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: DB assertion turetilebilirlik olgularini ve birlesik kapinin kararini tasir.
// sistemdeki gorevi: Manager'in tum assertion'lari birlikte fail-closed degerlendirmesini saglar.
public sealed class DatabaseDerivabilityResult
{
    public List<DatabaseDerivabilityItem> Assertions { get; set; } = [];
    public bool AllDerivable { get; set; }
}
