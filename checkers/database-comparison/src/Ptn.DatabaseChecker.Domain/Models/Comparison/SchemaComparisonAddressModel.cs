namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Diff motorunun tek bulguyu hangi sema/nesne/alt nesne adresine yazacagini tasir.
// sistemdeki gorevi: Generic koleksiyon comparer ayni adres modelini kullanarak tablo, kolon, index, constraint, trigger ve schema-level nesneler icin SchemaDifferenceModel uretir.
public class SchemaComparisonAddressModel
{
    // Farkin bulundugu sema adi.
    public string SchemaName { get; set; } = string.Empty;

    // Farkin bulundugu ana nesne adi; kolon/index/constraint/trigger icin tablo adidir.
    public string ObjectName { get; set; } = string.Empty;

    // Farkin bulundugu alt nesne adi; tablo/view gibi ana nesne farklarinda null kalir.
    public string? ChildName { get; set; }

    // Fark nesnesinin kararli tur kodu (SchemaObjectTypeCodes.*).
    public string ObjectTypeCode { get; set; } = string.Empty;
}
