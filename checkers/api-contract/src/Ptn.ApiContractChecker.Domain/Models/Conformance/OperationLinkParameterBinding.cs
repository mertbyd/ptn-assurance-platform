namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Kaynak response JSON Pointer'ini hedef operasyon parametre adina baglayan tek oneriyi tasir.
// sistemdeki gorevi: Operasyon zinciri adayinin alan seviyesindeki mekanik gerekcesini aciklar.
public sealed class OperationLinkParameterBinding
{
    public string SourceResponsePointer { get; }
    public string TargetParameterName { get; }

    // Kaynak pointer ile hedef parametre adini degismez esleme olarak kurar.
    public OperationLinkParameterBinding(string sourceResponsePointer, string targetParameterName)
    {
        SourceResponsePointer = sourceResponsePointer;
        TargetParameterName = targetParameterName;
    }
}
