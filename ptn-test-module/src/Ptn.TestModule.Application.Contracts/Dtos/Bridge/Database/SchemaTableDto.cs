namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Sema snapshot'indaki tek tabloyu tasir.
// sistemdeki gorevi: Fingerprint girdisini kararli tablo ve kolon navigasyonuyla sunar.
public sealed class SchemaTableDto
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SchemaColumnDto> Columns { get; set; } = [];
}
