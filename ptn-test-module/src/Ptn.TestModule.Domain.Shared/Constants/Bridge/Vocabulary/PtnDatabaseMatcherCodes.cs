using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Test Module kolon beklentilerinin kullanabilecegi kapali matcher kod kumesini tanimlar.
// sistemdeki gorevi: Ajan yazarlik kapi denetimlerinde, kapali secenek listelerinde ve DTO dogrulamalarinda kullanilir.
public static class PtnDatabaseMatcherCodes
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
    
    public static readonly IReadOnlyList<string> All =
    [
        Equals, NotEquals, IsNull, IsNotNull, 
        GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,
        MatchesRegex, OneOf, WithinTolerance
    ];
}
