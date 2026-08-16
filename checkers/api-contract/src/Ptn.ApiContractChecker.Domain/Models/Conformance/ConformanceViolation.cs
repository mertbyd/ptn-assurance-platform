namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Tek uygunluk ihlalini kural, RFC 6901 adresi ve sema anahtar sozcugu ile tasir.
// sistemdeki gorevi: Gozlenen veya beklenen degeri HTTP sinirina cikarmayan kapali ihlal ucusudur.
public sealed class ConformanceViolation
{
    public string RuleCode { get; }
    public string JsonPointer { get; }
    public string Keyword { get; }
    internal string LevelCode { get; }
    internal string OutcomeCode { get; }

    public ConformanceViolation(
        string ruleCode,
        string jsonPointer,
        string keyword,
        string levelCode,
        string outcomeCode)
    {
        RuleCode = ruleCode;
        JsonPointer = jsonPointer;
        Keyword = keyword;
        LevelCode = levelCode;
        OutcomeCode = outcomeCode;
    }
}
