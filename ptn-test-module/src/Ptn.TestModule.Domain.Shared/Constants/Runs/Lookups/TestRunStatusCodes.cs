using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Kosum motorunun yasam dongusu durumlarini kararli kodlarla tanimlar.
// sistemdeki gorevi: test_run_statuses lookup'inin kapali sozlugudur; seed ve FK cozumlemesi bu kodlardan yurur (ADR-0016 §F).
public static class TestRunStatusCodes
{
    // Motor henuz baslamadi; kosu sirada bekliyor.
    public const string Pending = "Pending";

    // Motor calisiyor.
    public const string Running = "Running";

    // Motor duzgun bitti; icinde basarisiz test OLABILIR, hukum test_outcome_statuses'tadir.
    public const string Completed = "Completed";

    // Kosu disaridan iptal edildi.
    public const string Cancelled = "Cancelled";

    // Motorun kendisi coktu.
    public const string Aborted = "Aborted";

    // Kosu sure sinirini asti.
    public const string TimedOut = "TimedOut";

    public static IReadOnlyCollection<string> All { get; } =
        [Pending, Running, Completed, Cancelled, Aborted, TimedOut];
}
