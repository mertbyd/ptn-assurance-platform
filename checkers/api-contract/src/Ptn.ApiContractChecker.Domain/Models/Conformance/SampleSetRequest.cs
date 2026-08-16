namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Snapshot operasyonu, ornek turu ve alan butcesini domain uretim akisina tasir.
// sistemdeki gorevi: HTTP DTO'sunu manager'dan ayirarak tahminsiz operasyon secimini korur.
public sealed class SampleSetRequest
{
    public string? OperationId { get; }
    public string Method { get; }
    public string Path { get; }
    public string SampleKindCode { get; }
    public int MaxSamplesPerField { get; }

    // Dogrulanmis ornek uretim girdisini degismez domain modeline kurar.
    public SampleSetRequest(
        string? operationId,
        string method,
        string path,
        string sampleKindCode,
        int maxSamplesPerField)
    {
        OperationId = operationId;
        Method = method;
        Path = path;
        SampleKindCode = sampleKindCode;
        MaxSamplesPerField = maxSamplesPerField;
    }
}
