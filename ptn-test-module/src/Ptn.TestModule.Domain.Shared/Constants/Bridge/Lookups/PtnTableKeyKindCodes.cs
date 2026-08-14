using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Lookups;

// islevi: Tablo taniminda kullanilan birincil ve tekil anahtar turlerini tanimlar.
// sistemdeki gorevi: KeyUnique kanitini checker metni veya index adindan bagimsiz tutar.
public static class PtnTableKeyKindCodes
{
    public const string Primary = nameof(Primary);
    public const string Unique = nameof(Unique);

    public static IReadOnlyCollection<string> All { get; } = [Primary, Unique];
}
