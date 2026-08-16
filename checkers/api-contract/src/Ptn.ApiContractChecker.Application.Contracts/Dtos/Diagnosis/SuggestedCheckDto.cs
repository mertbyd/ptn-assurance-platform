namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Composition host tarafindan cozulmek uzere tipli sonraki kontrol onerisi tasir.
// sistemdeki gorevi: Checker paketleri arasinda compile-time bagimlilik kurmadan teshis zinciri acar.
public sealed class SuggestedCheckDto
{
    public string CapabilityCode { get; set; } = string.Empty;
    public string OperationCode { get; set; } = string.Empty;
    public Dictionary<string, string?> Arguments { get; set; } = new(StringComparer.Ordinal);
}
