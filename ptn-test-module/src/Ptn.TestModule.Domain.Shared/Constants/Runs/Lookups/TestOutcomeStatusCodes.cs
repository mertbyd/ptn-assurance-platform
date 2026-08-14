using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Tek bir testin hukmunu kararli kodlarla tanimlar.
// sistemdeki gorevi: test_outcome_statuses lookup'inin kapali sozlugudur; build kirma politikasi koda degil breaks_build kolonuna baglanir (ADR-0016 §F).
public static class TestOutcomeStatusCodes
{
    // Ana yol kostu, her sey tuttu.
    public const string Passed = "Passed";

    // Hakem HAYIR dedi; gercek bulgu.
    public const string Failed = "Failed";

    // Beklenmeyen hata; testin kendisi kirildi.
    public const string Broken = "Broken";

    // Test bilerek atlandi.
    public const string Skipped = "Skipped";

    // On kosul saglanmadi; ana yol hic kosmadi, hicbir sey dogrulanmadi.
    public const string Inconclusive = "Inconclusive";

    public static IReadOnlyCollection<string> All { get; } =
        [Passed, Failed, Broken, Skipped, Inconclusive];
}
