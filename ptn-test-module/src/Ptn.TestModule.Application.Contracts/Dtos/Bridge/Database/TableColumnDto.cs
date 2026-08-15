namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek tablo kolonunun kanonik tip ve nullability bilgisini tasir.
// sistemdeki gorevi: Provider tipini kararli Bridge sozlugune indirger.
public sealed class TableColumnDto
{
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool IsNullable { get; set; }
}
