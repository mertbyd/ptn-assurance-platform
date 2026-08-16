namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Bir tablonun tek bir kolonunun API cevap modelidir.
// sistemdeki gorevi: Derin sema okumasinda kolonun adi, tipi ve kisitlarini frontend'e tasir; karsilastirma oncesi tam sema goruntusunu besler.
public class SchemaColumnDto
{
    /// <summary>
    /// Kolon adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Kolonun tablodaki sirasi (1'den baslar).
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Motorun yazdigi ham tip (or. "varchar(100)", "numeric(10,2)").
    /// </summary>
    public string RawDataType { get; set; } = default!;

    /// <summary>
    /// Kolon NULL kabul ediyor mu.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Metin/binary tipler icin azami uzunluk; uygulanamiyorsa bos.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Sayisal tipler icin toplam anlamli basamak sayisi; degilse bos.
    /// </summary>
    public int? NumericPrecision { get; set; }

    /// <summary>
    /// Sayisal tipler icin ondalik basamak sayisi; degilse bos.
    /// </summary>
    public int? NumericScale { get; set; }

    /// <summary>
    /// Oto-artan (identity) kolon mu.
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Kolon default degerinin motor uretimi SQL ifadesi.
    /// </summary>
    public string? DefaultValueSql { get; set; }

    /// <summary>
    /// Kolonun provider katalogundaki collation adi.
    /// </summary>
    public string? CollationName { get; set; }

    /// <summary>
    /// Kolon generated/computed mi.
    /// </summary>
    public bool IsGenerated { get; set; }

    /// <summary>
    /// Generated/computed kolon ifadesi.
    /// </summary>
    public string? GenerationExpression { get; set; }

    /// <summary>
    /// Generated deger fiziksel olarak saklaniyor mu.
    /// </summary>
    public bool IsPersisted { get; set; }

    /// <summary>
    /// Identity baslangic degeri.
    /// </summary>
    public string? IdentitySeed { get; set; }

    /// <summary>
    /// Identity artis miktari.
    /// </summary>
    public string? IdentityIncrement { get; set; }

    /// <summary>
    /// Kolon aciklamasi.
    /// </summary>
    public string? Comment { get; set; }
}
