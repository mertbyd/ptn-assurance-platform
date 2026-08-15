namespace Ptn.TestModule.Constants.Bridge;

// islevi: Kapali kanit hukum ifadelerinin operator ve alan son eklerini tanimlar.
// sistemdeki gorevi: EvidenceChainManager'in serbest kod calistirmadan sinirli ifadeleri degerlendirmesini saglar.
public static class PtnEvidenceExpressionTokens
{
    public const string Negation = "!";
    public const string ObservedSuffix = ".observed";
    public const string ContainsAny = ".containsAny(";
    public const string ValuesSuffix = ".values)";
}
