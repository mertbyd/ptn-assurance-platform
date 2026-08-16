namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Motorun buldugu tek bir yapi farki (bir eleman = bir fark); adres uclusu (nerede) + siniflandirma uclusu (ne) + kanit uclusu (nasil gorunuyor).
// sistemdeki gorevi: Eski SchemaDifference tablosunun owned-JSON karsiligi; ComparisonFindings.SchemaDifferences icinde tasinir. Lookup'lara FK yerine kararli Code ile baglanir (donmus snapshot).
public class SchemaDifferenceModel
{
    /// <summary>
    /// Ayni semantik farki kosular arasinda kararli olarak tanimlayan SHA-256 degeri.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Farkin uyumluluk etkisini belirleyen kararli siddet kodu.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    // Farkin bulundugu sema (adres uclusunun ilk parcasi).
    public string SchemaName { get; set; } = default!;

    // Farkin bulundugu nesne (tablo/view/index...).
    public string ObjectName { get; set; } = default!;

    // Nesnenin icindeki parca (kolon adi gibi); tablo seviyesi farkta null.
    public string? ChildName { get; set; }

    // Nesne turunun kararli kodu (SchemaObjectTypeCodes.*): Table, View, Column, Index...; rapor bununla gruplar.
    public string ObjectTypeCode { get; set; } = default!;

    // Fark yonunun kararli kodu (DifferenceKindCodes.*): OnlyInSource / OnlyInTarget / Modified.
    public string KindCode { get; set; } = default!;

    // Fark guveninin kararli kodu (ComparisonConfidenceCodes.*): Exact / Canonical / Approximate / Incomparable.
    public string ConfidenceCode { get; set; } = default!;

    // Nesnenin kaynaktaki kanonik tanim metni; OnlyInTarget'ta null (kaynakta yok).
    public string? SourceDefinition { get; set; }

    // Ayni tanimin hedefteki hali; OnlyInSource'ta null.
    public string? TargetDefinition { get; set; }

    // Modified'da neyin degistiginin kisa listesi ("Nullability, Length"); diger yonlerde null.
    public string? ChangeSummary { get; set; }
}
