namespace Ptn.TestModule.Constants.Bridge;

// islevi: Profil paketindeki kanit hukum dilinin izin verilen kapali ifade kaliplarini tanimlar.
// sistemdeki gorevi: Profil verisinin serbest kod veya genisletilebilir betik diline donusmesini engeller.
public static class PtnEvidenceExpressionPatterns
{
    public const string Observed = @"^[A-Za-z][A-Za-z0-9]*\.observed$";
    public const string ContainsAny =
        @"^!?[A-Za-z][A-Za-z0-9]*\.containsAny\([A-Za-z][A-Za-z0-9]*\.values\)$";
    public const string Unavailable = @"^any\(step\.state == Unavailable\)$";
    public const string AndSeparator = " && ";
}
