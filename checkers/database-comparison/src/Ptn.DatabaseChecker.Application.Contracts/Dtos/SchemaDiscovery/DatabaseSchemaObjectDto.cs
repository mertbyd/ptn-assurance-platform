namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Bir semadaki tek nesnenin (tablo/view/trigger/function/procedure) API cevap modelidir.
// sistemdeki gorevi: T4 nesne listeleme ucunun ciktisi; nesne adini ve kararli tur kodunu tasir, frontend tur kodunu yukledigi lookup listesiyle etikete cevirir.
public class DatabaseSchemaObjectDto
{
    /// <summary>
    /// Nesnenin ait oldugu sema adi.
    /// </summary>
    public string Schema { get; set; } = default!;

    /// <summary>
    /// Nesne adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Nesne turunun kararli kodu (SchemaObjectTypeCodes.*): Table/View/Trigger/Function/Procedure.
    /// </summary>
    public string ObjectTypeCode { get; set; } = default!;
}
