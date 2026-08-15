using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Response uygunluk kural seviyesini secen kapali Bridge profil kodlarini tanimlar.
// sistemdeki gorevi: Senaryo yazarligi ile API checker profil sozlesmesini tek kararli sozlukte bulusturur.
public static class PtnConformanceProfileCodes
{
    public const string Strict = nameof(Strict);
    public const string Runtime = nameof(Runtime);
    public const string Lenient = nameof(Lenient);

    public static IReadOnlyCollection<string> All { get; } = [Strict, Runtime, Lenient];
}
