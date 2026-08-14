namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Bulguyu kaynak checker ve kararli fingerprint ile tanimlar.
// sistemdeki gorevi: Hipotez ve kanitlarin izlenebilir public referansini verir.
public sealed class FindingRefDto
{
    public string SourceCheckerCode { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}
