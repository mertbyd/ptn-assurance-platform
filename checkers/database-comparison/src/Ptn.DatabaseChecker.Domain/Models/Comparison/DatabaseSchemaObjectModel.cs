namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir semadaki tek bir nesnenin (tablo/view/trigger/function/procedure) hafif kesif temsilidir.
// sistemdeki gorevi: T4 nesne listeleme akisinda motor-bagimsiz nesne adi + tur kodunu tasir; native katalog turleri (relkind/prokind/sys.objects.type) okuyucuda SchemaObjectTypeCodes'a cevrilir, cagiran motoru bilmez.
public class DatabaseSchemaObjectModel
{
    // Nesnenin ait oldugu sema adi; nesne adiyla birlikte kesif listesinin kimligidir.
    public string Schema { get; set; } = string.Empty;

    // Nesne adi (tablo/view/trigger/function/procedure adi).
    public string Name { get; set; } = string.Empty;

    // Nesne turunun kararli kodu (SchemaObjectTypeCodes.*); native katalog kodundan cevrilir.
    public string ObjectTypeCode { get; set; } = string.Empty;
}
