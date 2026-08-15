namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Kaynak response pointer'i ile hedef parametre adini public Bridge cevabinda tasir.
// sistemdeki gorevi: Arazzo baglama bilgisini checker tipinden bagimsiz sunar.
public sealed class OperationLinkParameterBindingDto
{
    public string SourceResponsePointer { get; set; } = string.Empty;
    public string TargetParameterName { get; set; } = string.Empty;
}
