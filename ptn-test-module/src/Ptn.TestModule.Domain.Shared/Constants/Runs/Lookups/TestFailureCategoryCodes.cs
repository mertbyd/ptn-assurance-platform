using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Bir bulgunun hangi hakemden geldigini kararli kodlarla tanimlar.
// sistemdeki gorevi: test_failure_categories lookup'inin kapali sozlugudur; bulgu kaynagini checker modulunden bagimsiz adlandirir (ADR-0016 §F).
public static class TestFailureCategoryCodes
{
    // API Contract Checker hayir dedi.
    public const string Contract = "Contract";

    // Database Checker hayir dedi.
    public const string Persistence = "Persistence";

    // Is degismezi ihlal edildi.
    public const string Business = "Business";

    // Ag / HTTP seviyesinde hata.
    public const string Transport = "Transport";

    // Beklenmeyen exception'in guvenli karsiligi.
    public const string Technical = "Technical";

    public static IReadOnlyCollection<string> All { get; } =
        [Contract, Persistence, Business, Transport, Technical];
}
