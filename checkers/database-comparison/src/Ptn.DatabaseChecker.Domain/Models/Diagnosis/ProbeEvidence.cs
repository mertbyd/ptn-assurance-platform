using Ptn.DatabaseChecker.Constants.Diagnosis;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Tek salt-okuma probe veya katalog olgusunun kararli sonuc kodunu, sayimini ve redaction uygulanmis degerini tasir.
// sistemdeki gorevi: Hipotez degerlendirmesini aciklama metninden ayirir ve raporda hipotez basina sinirli kanit verir.
public sealed class ProbeEvidence
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string HypothesisKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public long? ObservedCount { get; set; }
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }

    // islevi: Probe gerektirmeyen canli katalog olgusunu hipoteze bagli yapilandirilmis kanita cevirir.
    public static ProbeEvidence Catalog(
        string hypothesisKindCode,
        string? expectedValue = null,
        string? observedValue = null)
        => new()
        {
            HypothesisKindCode = hypothesisKindCode,
            FactCode = ProbeKindCodes.Facts.Catalog,
            ExpectedValue = expectedValue,
            ObservedValue = observedValue
        };

    // islevi: Bir hipotezin belirli probe turune ait tamamlanmis kanitini koleksiyondan bulur.
    public static ProbeEvidence? Find(
        System.Collections.Generic.List<ProbeEvidence> evidence,
        string hypothesisKindCode,
        string probeKindCode)
        => evidence.Find(item =>
            item.HypothesisKindCode == hypothesisKindCode && item.ProbeKindCode == probeKindCode);
}
