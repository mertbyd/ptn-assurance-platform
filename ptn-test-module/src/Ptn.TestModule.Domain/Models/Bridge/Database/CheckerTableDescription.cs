using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker tablo taniminin kaynak alan seklini tasir.
// sistemdeki gorevi: SchemaName semantigini Manager'a birakip Mapperly eslemesini attribute'suz tutar.
public sealed class CheckerTableDescription
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<TableColumn> Columns { get; set; } = [];
    public TableKey? PrimaryKey { get; set; }
    public List<TableKey> UniqueIndexes { get; set; } = [];
    public List<CheckerForeignKeyNeighbor> ForeignKeyNeighbors { get; set; } = [];
    public List<CheckerSchemaLintWarning> LintWarnings { get; set; } = [];
}
