namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Bir conformance kuralinin sonuc etkisini belirleyen kapali seviye kodlarini tanimlar.
// sistemdeki gorevi: Ignore, Info, Warn ve Fail davranisini profil resolver'i ile sonuc toplayicisinda ortaklastirir.
public static class ConformanceLevelCodes
{
    public const string Ignore = "Ignore";
    public const string Info = "Info";
    public const string Warn = "Warn";
    public const string Fail = "Fail";

    public static IReadOnlyCollection<string> All { get; } = [Ignore, Info, Warn, Fail];
}
