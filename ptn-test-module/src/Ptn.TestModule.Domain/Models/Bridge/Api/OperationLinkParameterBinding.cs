namespace Ptn.TestModule.Models.Bridge;

// islevi: Kaynak response pointer'i ile hedef operasyon parametresinin mekanik bagini tasir.
// sistemdeki gorevi: Arazzo adim zincirinin alan aktarimini serbest metin tahmininden ayirir.
public sealed class OperationLinkParameterBinding
{
    public string SourceResponsePointer { get; set; } = string.Empty;
    public string TargetParameterName { get; set; } = string.Empty;
}
