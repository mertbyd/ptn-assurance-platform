using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Is degismezi degerlendiricisinin kapali desen kodlarini tanimlar (RESEARCH-0009 M-1..M-4).
// sistemdeki gorevi: Alan bilgisi tasimadan hangi aritmetik karsilastirmanin kosacagini kapali kumeye baglar.
public static class PtnInvariantPatternCodes
{
    // M-1 Korunum: bir buyukluk islem boyunca degismez; left == right.
    public const string Conservation = "Conservation";

    // M-2 Delta: beklenen tam degisim; right - left == delta.
    public const string Delta = "Delta";

    // M-3 Tutarlilik: bagimsiz turetilmis iki gorunum ayni degeri verir; left == right.
    public const string Consistency = "Consistency";

    // M-4 Tekillik: gozlenen yinelenme sayisi sifirdir; left == 0.
    public const string Uniqueness = "Uniqueness";

    public static readonly IReadOnlyList<string> All = [Conservation, Delta, Consistency, Uniqueness];
}
