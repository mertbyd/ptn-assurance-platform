namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Tek hedef operasyonu kaynak kanit, parametre eslemeleri ve guven skoruyla tasir.
// sistemdeki gorevi: Checker'in yalniz aday uretmesini ve son karari her zaman insana birakmasini saglar.
public sealed class OperationLinkCandidate
{
    public string TargetOperationId { get; }
    public string SourceCode { get; }
    public List<OperationLinkParameterBinding> ParameterMap { get; }
    public decimal Score { get; }
    public bool RequiresHumanApproval { get; } = true;

    // Kanitli adayi insan onayi zorunlulugunu degistirilemez tutarak kurar.
    public OperationLinkCandidate(
        string targetOperationId,
        string sourceCode,
        List<OperationLinkParameterBinding> parameterMap,
        decimal score)
    {
        TargetOperationId = targetOperationId;
        SourceCode = sourceCode;
        ParameterMap = parameterMap;
        Score = score;
    }
}
