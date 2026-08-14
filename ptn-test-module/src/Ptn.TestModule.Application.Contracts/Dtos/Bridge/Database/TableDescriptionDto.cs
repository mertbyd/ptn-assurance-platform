namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Bir tablonun kolon ve anahtar ozetini tasir.
// sistemdeki gorevi: Provider-bagimsiz sema sonucunu public Bridge kontratinda sunar.
public sealed class TableDescriptionDto
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<TableColumnDto> Columns { get; set; } = [];
    public TableKeyDto? PrimaryKey { get; set; }
    public List<TableKeyDto> UniqueIndexes { get; set; } = [];
}
