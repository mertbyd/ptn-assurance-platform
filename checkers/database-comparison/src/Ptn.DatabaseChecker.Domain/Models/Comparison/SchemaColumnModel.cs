using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek bir kolonun kanonik temsili; tipi iki katmanli tasir (ham + kanonik, 02 no'lu belge).
// sistemdeki gorevi: Ayni-motor karsilastirmasi RawDataType ile Exact, capraz-motor karsilastirmasi CanonicalDataType ve TypeMappingFidelityCode ile yapilir.
public class SchemaColumnModel : ExtensibleObject
{
    // Kolon adi; tablo icinde diff eslestirme anahtaridir.
    public string Name { get; set; } = string.Empty;

    // Kolonun tablodaki sirasi (1'den baslar); sira farki da raporlanabilir bir bulgudur.
    public int Ordinal { get; set; }

    // Motorun yazdigi ham tip (or. "nvarchar(100)", "timestamp(6) with time zone"); ayni-motor kiyasinda kesin olcut, raporda kanit olarak gosterilir.
    public string RawDataType { get; set; } = string.Empty;

    // Motordan bagimsiz ortak tip karsiligi (or. "String", "Integer"); capraz kiyasin olcutu. Tip esleme adiminda doldurulur.
    public string? CanonicalDataType { get; set; }

    // Ham tipin kanonik aileye donusum fidelity kodu; capraz-motor bulgusunun Canonical/Approximate kararini tasir.
    public string? TypeMappingFidelityCode { get; set; }

    // Kolonun NULL kabul edip etmedigi; isterlerdeki "Nullable durumu farki" dogrudan bu alandan cikar.
    public bool IsNullable { get; set; }

    // Metin/binary tipler icin azami uzunluk; tipe uygulanamiyorsa null (isterlerdeki "boyut farki").
    public int? MaxLength { get; set; }

    // Sayisal tipler icin toplam anlamli basamak sayisi (or. numeric(10,2) -> 10); degilse null.
    public int? NumericPrecision { get; set; }

    // Sayisal tipler icin ondalik basamak sayisi (or. numeric(10,2) -> 2); degilse null.
    public int? NumericScale { get; set; }

    // Oto-artan kimlik kolonu mu (PG: identity/serial, MSSQL: IDENTITY); iki motorda gerceklesme farkli, anlam ortak.
    public bool IsIdentity { get; set; }

    // Varsayilan degerin motor uretimi SQL ifadesi; ham tasinir, yalanci farklari eleyen normalizasyon 7. gorevin isi.
    public string? DefaultValueSql { get; set; }

    // Metin kolonunun provider katalogundaki collation adi; collatable olmayan tiplerde null kalir.
    public string? CollationName { get; set; }

    // Kolon degeri provider tarafindan bir ifadeden uretiliyor mu (PG generated / MSSQL computed).
    public bool IsGenerated { get; set; }

    // Generated/computed kolonun provider tarafindan geri uretilen ifade metni.
    public string? GenerationExpression { get; set; }

    // Generated deger fiziksel olarak saklaniyor mu; desteklenmeyen motorlarda false bilgi siniridir.
    public bool IsPersisted { get; set; }

    // Identity sequence'in baslangic degeri; identity olmayan veya provider'in bildirmedigi kolonda null kalir.
    public string? IdentitySeed { get; set; }

    // Identity sequence'in artis miktari; identity olmayan veya provider'in bildirmedigi kolonda null kalir.
    public string? IdentityIncrement { get; set; }

    // Kolon aciklamasi (PG pg_description / MSSQL MS_Description); yoksa null kalir.
    public string? Comment { get; set; }
}
