using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Lookups;

// islevi: Kanit dugumunun uc degerli gozlem durumunu tanimlar.
// sistemdeki gorevi: Kanitin okunamamasi ile olgunun gozlenmemesi arasindaki ayrimi zorunlu kilar.
public static class PtnEvidenceStateCodes
{
    public const string Observed = nameof(Observed);
    public const string NotObserved = nameof(NotObserved);
    public const string Unavailable = nameof(Unavailable);

    public static IReadOnlyCollection<string> All { get; } = [Observed, NotObserved, Unavailable];
}
