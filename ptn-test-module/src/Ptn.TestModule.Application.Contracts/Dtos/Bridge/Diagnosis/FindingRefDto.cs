namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Bulguyu kaynak checker ve kararli fingerprint ile tanimlar.
// sistemdeki gorevi: Hipotez ve kanitlarin izlenebilir public referansini verir.
public sealed class FindingRefDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string SourceCheckerCode { get; set; } = string.Empty;
    /// <summary>
    /// Sema veya profil iceriginin kararli parmak izini belirtir.
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;
}
