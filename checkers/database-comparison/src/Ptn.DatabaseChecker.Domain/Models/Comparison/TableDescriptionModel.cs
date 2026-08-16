using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Senaryo yazim aninda tek tablonun kolon, anahtar, FK komsuluk ve lint ozetini tasir.
// sistemdeki gorevi: Maliyetli tam snapshot'i Test Module'a kucuk ve amaca yonelik bir tablo tanimlama cevabi olarak sunar.
public sealed class TableDescriptionModel
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<TableDescriptionColumnModel> Columns { get; set; } = new();
    public TableKeyDefinitionModel? PrimaryKey { get; set; }
    public List<TableKeyDefinitionModel> UniqueIndexes { get; set; } = new();
    public List<ForeignKeyNeighborModel> ForeignKeyNeighbors { get; set; } = new();
    public List<SchemaLintWarningModel> LintWarnings { get; set; } = new();
}
