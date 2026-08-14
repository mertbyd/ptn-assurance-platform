namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tekil veya birincil tablo anahtarinin kolonlarini tasir.
// sistemdeki gorevi: Anahtar bilgisini provider DTO'sundan bagimsiz sunar.
public sealed class TableKeyDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
}
