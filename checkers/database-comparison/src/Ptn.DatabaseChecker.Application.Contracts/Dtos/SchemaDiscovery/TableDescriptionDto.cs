using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Senaryo yaziminda tek tablonun kolon, anahtar, FK komsuluk ve lint ozetini tasir.
// sistemdeki gorevi: Maliyetli full snapshot cevabi yerine assertion sozlesmesine yonelik kucuk tablo tanimlama API cevabidir.
public class TableDescriptionDto
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<TableDescriptionColumnDto> Columns { get; set; } = new();
    public TableKeyDefinitionDto? PrimaryKey { get; set; }
    public List<TableKeyDefinitionDto> UniqueIndexes { get; set; } = new();
    public List<ForeignKeyNeighborDto> ForeignKeyNeighbors { get; set; } = new();
    public List<SchemaLintWarningDto> LintWarnings { get; set; } = new();
}
