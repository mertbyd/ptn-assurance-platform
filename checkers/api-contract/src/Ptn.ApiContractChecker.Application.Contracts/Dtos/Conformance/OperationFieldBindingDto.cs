namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Onerilen kaynak ve hedef alan pointer'lariyla uyumlu tipi HTTP cikisina tasir.
public class OperationFieldBindingDto
{
    public string SourcePointer { get; set; } = string.Empty;
    public string TargetPointer { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Score { get; set; }
    public string Expression { get; set; } = string.Empty;
}
