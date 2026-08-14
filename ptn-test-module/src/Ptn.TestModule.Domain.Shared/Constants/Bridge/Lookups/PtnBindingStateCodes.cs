using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Lookups;

// islevi: Profil paketi kavram baglamalarinin onay durumlarini tanimlar.
// sistemdeki gorevi: Sema kaymasinda baglamayi sessizce kullanmak yerine yeniden onaya dusurur.
public static class PtnBindingStateCodes
{
    public const string Proposed = nameof(Proposed);
    public const string Approved = nameof(Approved);
    public const string Rejected = nameof(Rejected);

    public static IReadOnlyCollection<string> All { get; } = [Proposed, Approved, Rejected];
}
