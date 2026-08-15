namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Sema snapshot'indaki tek tabloyu tasir.
// sistemdeki gorevi: Fingerprint girdisini kararli tablo ve kolon navigasyonuyla sunar.
public sealed class SchemaTableDto
{
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string Schema { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<SchemaColumnDto> Columns { get; set; } = [];
}
