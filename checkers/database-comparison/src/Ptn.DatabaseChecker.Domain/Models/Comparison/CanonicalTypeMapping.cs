namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir ham veritabani tipinin kanonik aile kodunu ve donusum fidelity kodunu birlikte tasir.
// sistemdeki gorevi: Motor-ozel tip saglayicisinin tek seferlik normalizasyon sonucunu discovery modeline kayipsiz aktarir.
public class CanonicalTypeMapping
{
    // islevi: Kanonik tip ailesi ile fidelity kodunu tek ve degismez esleme sonucu olarak kurar.
    public CanonicalTypeMapping(string canonicalTypeCode, string fidelityCode)
    {
        CanonicalTypeCode = canonicalTypeCode;
        FidelityCode = fidelityCode;
    }

    // CanonicalDataTypeCodes altindaki motor-bagimsiz tip ailesi.
    public string CanonicalTypeCode { get; }

    // TypeMappingFidelityCodes altindaki tek-taraf donusum guveni.
    public string FidelityCode { get; }
}
