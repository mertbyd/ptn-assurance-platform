using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge;

// islevi: Bridge cevaplarinin kisa ve ayrintili bicim kodlarini tanimlar.
// sistemdeki gorevi: Tool yanit boyutunu serbest metin yerine dogrulanabilir kapali secimle sinirlar.
public static class PtnResponseFormatCodes
{
    public const string Concise = "concise";
    public const string Detailed = "detailed";

    public static IReadOnlyCollection<string> All { get; } = [Concise, Detailed];
}
