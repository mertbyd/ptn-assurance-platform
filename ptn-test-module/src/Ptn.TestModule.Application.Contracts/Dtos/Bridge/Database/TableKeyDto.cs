namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tekil veya birincil tablo anahtarinin kolonlarini tasir.
// sistemdeki gorevi: Anahtar bilgisini provider DTO'sundan bagimsiz sunar.
public sealed class TableKeyDto
{
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<string> Columns { get; set; } = [];
}
