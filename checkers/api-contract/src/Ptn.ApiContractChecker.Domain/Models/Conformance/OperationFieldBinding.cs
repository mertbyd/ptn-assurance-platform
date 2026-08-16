namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Kaynak response alanindan hedef request alanina isim ve tip uyumlu tek oneriyi tasir.
// sistemdeki gorevi: ODG kenarinin neden onerildigini iki JSON pointer ile aciklar.
public sealed class OperationFieldBinding
{
    public string SourcePointer { get; }
    public string TargetPointer { get; }
    public string? Type { get; }
    public int Score { get; }
    public string Expression { get; }

    public OperationFieldBinding(
        string sourcePointer,
        string targetPointer,
        string? type,
        int score,
        string expression)
    {
        SourcePointer = sourcePointer;
        TargetPointer = targetPointer;
        Type = type;
        Score = score;
        Expression = expression;
    }
}
