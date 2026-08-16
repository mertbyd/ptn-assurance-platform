namespace Ptn.DatabaseChecker.Constants.Comparison.Assertions;

// islevi: Test Module kolon beklentilerinin kullanabilecegi kapali matcher kod kumesini tanimlar.
// sistemdeki gorevi: DTO dogrulamasi, saf matcher ve KBP-705 teshis girdisi ayni kararli kodlari kullanir.
public static class MatcherKindCodes
{
    public new const string Equals = "Equals";
    public const string NotEquals = "NotEquals";
    public const string IsNull = "IsNull";
    public const string IsNotNull = "IsNotNull";
    public const string GreaterThan = "GreaterThan";
    public const string GreaterThanOrEqual = "GreaterThanOrEqual";
    public const string LessThan = "LessThan";
    public const string LessThanOrEqual = "LessThanOrEqual";
    public const string MatchesRegex = "MatchesRegex";
    public const string OneOf = "OneOf";
    public const string WithinTolerance = "WithinTolerance";

    // islevi: Bir matcher kodunun kapali assertion sozlesmesinde bulunup bulunmadigini bildirir.
    public static bool IsDefined(string? code)
        => code is Equals or NotEquals or IsNull or IsNotNull
            or GreaterThan or GreaterThanOrEqual or LessThan or LessThanOrEqual
            or MatchesRegex or OneOf or WithinTolerance;
}
