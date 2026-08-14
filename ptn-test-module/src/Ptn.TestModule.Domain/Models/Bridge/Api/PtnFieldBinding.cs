namespace Ptn.TestModule.Models.Bridge;

// islevi: Kaynak ve hedef JSON pointer arasindaki tek mekanik alan bagini tasir.
// sistemdeki gorevi: Ajanin serbest alan adi yazmasi yerine checker tarafindan onerilen referansi tasir.
public sealed class PtnFieldBinding
{
    public string SourcePointer { get; set; } = string.Empty;
    public string TargetPointer { get; set; } = string.Empty;
    public string? TypeCode { get; set; }
    public int Score { get; set; }
    public string Expression { get; set; } = string.Empty;
}
