namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Kaynak response pointer'i ile hedef parametre adini public response'a tasir.
// sistemdeki gorevi: Operasyon adayinin alan seviyesindeki mekanik bagini aciklar.
public class OperationLinkParameterBindingDto
{
    public string SourceResponsePointer { get; set; } = string.Empty;
    public string TargetParameterName { get; set; } = string.Empty;
}
