namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Tek bir yapi farkinin API modelidir (ComparisonRunDetailDto.Findings icinde).
// sistemdeki gorevi: SchemaDifferenceModel'in cevap karsiligi; lookup'lara FK yerine kararli Code tasir, FE kodlari yuklu lookup listelerinden isme cozer.
public class SchemaDifferenceDto
{
    /// <summary>
    /// Kosular arasinda kararli bulgu parmak izi.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Bulguyu Breaking, NonBreaking, Warning veya DocsOnly olarak siniflandiran kod.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    /// <summary>
    /// Farkin bulundugu sema adi.
    /// </summary>
    public string SchemaName { get; set; } = default!;

    /// <summary>
    /// Farkin bulundugu nesne adi (tablo/view/index...).
    /// </summary>
    public string ObjectName { get; set; } = default!;

    /// <summary>
    /// Nesnenin icindeki parca (kolon adi gibi); tablo seviyesi farkta null.
    /// </summary>
    public string? ChildName { get; set; }

    /// <summary>
    /// Nesne turunun kararli kodu (SchemaObjectTypeCodes.*).
    /// </summary>
    public string ObjectTypeCode { get; set; } = default!;

    /// <summary>
    /// Fark yonunun kararli kodu (DifferenceKindCodes.*).
    /// </summary>
    public string KindCode { get; set; } = default!;

    /// <summary>
    /// Fark guveninin kararli kodu (ComparisonConfidenceCodes.*).
    /// </summary>
    public string ConfidenceCode { get; set; } = default!;

    /// <summary>
    /// Nesnenin kaynaktaki kanonik tanim metni; OnlyInTarget'ta null.
    /// </summary>
    public string? SourceDefinition { get; set; }

    /// <summary>
    /// Ayni tanimin hedefteki hali; OnlyInSource'ta null.
    /// </summary>
    public string? TargetDefinition { get; set; }

    /// <summary>
    /// Modified'da neyin degistiginin kisa listesi; diger yonlerde null.
    /// </summary>
    public string? ChangeSummary { get; set; }
}
