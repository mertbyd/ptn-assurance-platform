namespace Ptn.TestModule.Models.Bridge;

// islevi: Bir kanit yolu adiminda checker'dan alinan olguyu ve kaynagini tasir.
// sistemdeki gorevi: Aciklama agacina yalniz alintilanabilir ve izlenebilir kanitin girmesini saglar.
public sealed class Evidence
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
    public long? ObservedAtMs { get; set; }
    public FindingRef? Ref { get; set; }
}
