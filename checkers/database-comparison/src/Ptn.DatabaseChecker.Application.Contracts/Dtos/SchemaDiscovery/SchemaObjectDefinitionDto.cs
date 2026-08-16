namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Snapshot icindeki tablo disi nesnelerin API cevap karsiligidir.
// sistemdeki gorevi: View/function/procedure/sequence/type/extension tanimlarini frontend'e tek kararli sozlesmeyle tasir.
public class SchemaObjectDefinitionDto
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
    /// Nesne turunun kararli kodu (SchemaObjectTypeCodes.*).
    /// </summary>
    public string ObjectTypeCode { get; set; } = default!;

    /// <summary>
    /// Nesnenin normalize edilmemis ham/okunabilir tanimi.
    /// </summary>
    public string Definition { get; set; } = default!;
}
