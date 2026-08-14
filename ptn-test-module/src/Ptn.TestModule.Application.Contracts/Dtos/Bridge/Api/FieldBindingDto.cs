namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Kaynak ve hedef JSON alanlari arasindaki tek baglamayi tasir.
// sistemdeki gorevi: Esleme ifadesini ve guven puanini public kontrata tasir.
public sealed class FieldBindingDto
{
    public string SourcePointer { get; set; } = string.Empty;
    public string TargetPointer { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Score { get; set; }
    public string Expression { get; set; } = string.Empty;
}
