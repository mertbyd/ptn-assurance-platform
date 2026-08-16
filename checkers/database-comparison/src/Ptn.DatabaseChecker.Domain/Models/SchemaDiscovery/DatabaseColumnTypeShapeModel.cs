namespace Ptn.DatabaseChecker.Models.SchemaDiscovery;

// islevi: Provider katalogundaki tip modifier alanlarini kanonik uzunluk, hassasiyet ve olcek degerlerine tasir.
// sistemdeki gorevi: PostgreSQL ve SQL Server kolon donusumlerinde cok degerli tuple ve ic ice tip kararlarini ortadan kaldirir.
/// <summary>
/// Provider tip modifier alanlarinin adlandirilmis uzunluk ve sayisal bicimidir.
/// </summary>
public sealed class DatabaseColumnTypeShapeModel
{
    /// <summary>
    /// Karakter veya ikili veri icin azami uzunluk.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Sayisal veri icin toplam hassasiyet.
    /// </summary>
    public int? NumericPrecision { get; init; }

    /// <summary>
    /// Sayisal veri icin ondalik olcek.
    /// </summary>
    public int? NumericScale { get; init; }

    /// <summary>
    /// Provider uzunluk alaninin sinirsiz MAX degerini temsil edip etmedigi.
    /// </summary>
    public bool IsMaxLength { get; init; }
}
